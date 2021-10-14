using BugsnagUnity.Payload;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BugsnagUnity
{
    internal class Client : IClient
    {
        public Configuration Configuration => NativeClient.Configuration;

        public IBreadcrumbs Breadcrumbs => NativeClient.Breadcrumbs;

        public ISessionTracker SessionTracking { get; }

        public LastRunInfo LastRunInfo => NativeClient.GetLastRunInfo();

        private User _cachedUser;

        private Metadata _storedMetadata;

        private UniqueLogThrottle _uniqueCounter;

        private MaximumLogTypeCounter _logTypeCounter;

        protected IDelivery Delivery => NativeClient.Delivery;

        object CallbackLock { get; } = new object();

        internal INativeClient NativeClient { get; }

        private Stopwatch _foregroundStopwatch;

        private Stopwatch _backgroundStopwatch;

        bool InForeground => _foregroundStopwatch.IsRunning;

        private Thread MainThread;

        private static double AutoCaptureSessionThresholdSeconds = 30;

        private static object autoSessionLock = new object();

        private static bool _contextSetManually;

        public Client(INativeClient nativeClient)
        {
            NativeClient = nativeClient;
            SessionTracking = new SessionTracker(this);
            MainThread = Thread.CurrentThread;
            InitStopwatches();
            InitUserObject();
            InitMetadata();
            InitCounters();
            SetupSceneLoadedBreadcrumbTracking();
            InitLogHandlers();       
            InitTimingTracker();
            InitInitialSessionCheck();          
            CheckForMisconfiguredEndpointsWarning();
            AddBugsnagLoadedBreadcrumb();
        }

        private void InitInitialSessionCheck()
        {
            // Run initial session check in next frame to allow potential configuration
            // changes to be completed first.
            try
            {
                var asyncHandler = MainThreadDispatchBehaviour.Instance();
                asyncHandler.Enqueue(RunInitialSessionCheck());
            }
            catch (System.Exception ex)
            {
                // Async behavior is not available in a test environment
            }
        }

        private void InitTimingTracker()
        {
            var timingTrackerObject = new GameObject("Bugsnag app lifecycle tracker");
            timingTrackerObject.AddComponent<TimingTrackerBehaviour>();
        }

        private void InitLogHandlers()
        {
            Application.logMessageReceivedThreaded += MultiThreadedNotify;
            Application.logMessageReceived += Notify;
        }

        private void InitCounters()
        {
            _uniqueCounter = new UniqueLogThrottle(Configuration);
            _logTypeCounter = new MaximumLogTypeCounter(Configuration);
        }

        private void InitMetadata()
        {
            _storedMetadata = new Metadata(NativeClient);
            _storedMetadata.MergeMetadata(Configuration.Metadata.Payload);
            AutomaticDataCollector.SetDefaultData(NativeClient);
        }

        private void InitStopwatches()
        {
            _foregroundStopwatch = new Stopwatch();
            _backgroundStopwatch = new Stopwatch();
        }

        private void InitUserObject()
        {
            if (Configuration.GetUser() != null)
            {
                _cachedUser = Configuration.GetUser();
            }
            else
            {
                _cachedUser = new User { Id = SystemInfo.deviceUniqueIdentifier };
            }
            NativeClient.PopulateUser(_cachedUser);
            _cachedUser.PropertyChanged.AddListener(() => { NativeClient.SetUser(_cachedUser); });
        }

        private void CheckForMisconfiguredEndpointsWarning()
        {
            var endpoints = Configuration.Endpoints;
            if (endpoints.IsValid)
            {
                return;
            }
            if (endpoints.NotifyIsCustom && !endpoints.SessionIsCustom)
            {
                UnityEngine.Debug.LogWarning("Invalid configuration. endpoints.Notify cannot be set without also setting endpoints.Session. Events will not be sent to Bugsnag.");
            }
            if (!endpoints.NotifyIsCustom && endpoints.SessionIsCustom)
            {
                UnityEngine.Debug.LogWarning("Invalid configuration. endpoints.Session cannot be set without also setting endpoints.Notify. Sessions will not be sent to Bugsnag.");
            }
        }

        private void AddBugsnagLoadedBreadcrumb()
        {
            if (IsUsingFallback() && Configuration.IsBreadcrumbTypeEnabled(BreadcrumbType.State))
            {
                Breadcrumbs.Leave("Bugsnag loaded", BreadcrumbType.State, null);
            }
        }

        public bool IsUsingFallback()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.WebGLPlayer:
                    return true;
            }
            return false;
        }

        private void SetupSceneLoadedBreadcrumbTracking()
        {
            if (!string.IsNullOrEmpty(NativeClient.Configuration.Context))
            {
                _contextSetManually = true;
            }
            if (Configuration.IsBreadcrumbTypeEnabled(BreadcrumbType.Navigation))
            {
                SceneManager.sceneLoaded += SceneLoaded;
            }
        }

        public void Send(IPayload payload)
        {
            if (!ShouldSendRequests())
            {
                return;
            }
            Delivery.Send(payload);
        }

        void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (!_contextSetManually)
            {
                Configuration.Context = scene.name;
                NativeClient.SetContext(scene.name);
            }
            Breadcrumbs.Leave("Scene Loaded", BreadcrumbType.State, new Dictionary<string, string> { { "sceneName", scene.name } });
        }

        void MultiThreadedNotify(string condition, string stackTrace, LogType logType)
        {
            // Discard messages from the main thread as they will be reported separately
            if (!object.ReferenceEquals(Thread.CurrentThread, MainThread))
            {
                Notify(condition, stackTrace, logType);
            }
        }

        /// <summary>
        /// Notify a Unity log message if it the client has been configured to
        /// notify at the specified level, if not leave a breadcrumb with the log
        /// message.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="stackTrace"></param>
        /// <param name="logType"></param>
        void Notify(string condition, string stackTrace, LogType logType)
        {

            if (Configuration.AutoDetectErrors && logType.IsGreaterThanOrEqualTo(Configuration.NotifyLogLevel) && Configuration.IsUnityLogErrorTypeEnabled(logType))
            {
                var logMessage = new UnityLogMessage(condition, stackTrace, logType);
                var shouldSend = Exception.ShouldSend(logMessage)
                  && _uniqueCounter.ShouldSend(logMessage)
                  && _logTypeCounter.ShouldSend(logMessage);
                if (shouldSend)
                {
                    var severity = Configuration.LogTypeSeverityMapping.Map(logType);
                    var backupStackFrames = new System.Diagnostics.StackTrace(1, true).GetFrames();
                    var forceUnhandled = logType == LogType.Exception && !Configuration.ReportUncaughtExceptionsAsHandled;
                    var exception = Exception.FromUnityLogMessage(logMessage, backupStackFrames, severity, forceUnhandled);
                    Notify(new Exception[] { exception }, exception.HandledState, null, logType);
                }
            }
            else if (Configuration.ShouldLeaveLogBreadcrumb(logType))
            {
                Breadcrumbs.Leave(logType.ToString(), BreadcrumbType.Log, new Dictionary<string, string> {
                    { "message", condition },
                });
            }
        }

        public void Notify(string name, string message, string stackTrace, OnErrorCallback callback)
        {
            var exceptions = new Exception[] { Exception.FromStringInfo(name, message, stackTrace) };
            Notify(exceptions, HandledState.ForHandledException(), callback, LogType.Exception);
        }

        public void Notify(System.Exception exception, string stacktrace, OnErrorCallback callback)
        {
            var exceptions = new Exceptions(exception, stacktrace).ToArray();
            Notify(exceptions, HandledState.ForHandledException(), callback, LogType.Exception);
        }

        public void Notify(System.Exception exception)
        {
            Notify(exception, 3);
        }

        internal void Notify(System.Exception exception, int level)
        {
            Notify(exception, HandledState.ForHandledException(), null, level);
        }

        public void Notify(System.Exception exception, OnErrorCallback callback)
        {
            Notify(exception, callback, 3);
        }

        internal void Notify(System.Exception exception, OnErrorCallback callback, int level)
        {
            Notify(exception, HandledState.ForHandledException(), callback, level);
        }

        public void Notify(System.Exception exception, Severity severity)
        {
            Notify(exception, severity, 3);
        }

        internal void Notify(System.Exception exception, Severity severity, int level)
        {
            Notify(exception, HandledState.ForUserSpecifiedSeverity(severity), null, level);
        }

        public void Notify(System.Exception exception, Severity severity, OnErrorCallback callback)
        {
            Notify(exception, severity, callback, 3);
        }

        internal void Notify(System.Exception exception, Severity severity, OnErrorCallback callback, int level)
        {
            Notify(exception, HandledState.ForUserSpecifiedSeverity(severity), callback, level);
        }

        void Notify(System.Exception exception, HandledState handledState, OnErrorCallback callback, int level)
        {
            // we need to generate a substitute stacktrace here as if we are not able
            // to generate one from the exception that we are given then we are not able
            // to do this inside of the IEnumerator generator code
            var substitute = new System.Diagnostics.StackTrace(level, true).GetFrames();
            Notify(new Exceptions(exception, substitute).ToArray(), handledState, callback, null);
        }

        void Notify(Exception[] exceptions, HandledState handledState, OnErrorCallback callback, LogType? logType)
        {
            if (!ShouldSendRequests() || EventContainsDiscardedClass(exceptions) || !Configuration.Endpoints.IsValid)
            {
                return;
            }

            var user = _cachedUser.Copy();

            var app = new AppWithState(Configuration)
            {
                InForeground = InForeground,
                DurationInForeground = _foregroundStopwatch.Elapsed,
            };

            NativeClient.PopulateAppWithState(app);

            var device = new DeviceWithState(Configuration);

            NativeClient.PopulateDeviceWithState(device);

            var eventMetadata = new Metadata();

            NativeClient.PopulateMetadata(eventMetadata);

            AutomaticDataCollector.AddStatefulDeviceData(eventMetadata);

            eventMetadata.MergeMetadata(_storedMetadata.Payload);

            RedactMetadata(eventMetadata);

            var @event = new Payload.Event(
              Configuration.Context,
              eventMetadata,
              app,
              device,
              user,
              exceptions,
              handledState,
              Breadcrumbs.Retrieve().ToList(),
              SessionTracking.CurrentSession);

            //Check for adding project packages to an android java error event
            if (ShouldAddProjectPackagesToEvent(@event))
            {
                @event.AddAndroidProjectPackagesToEvent(Configuration.ProjectPackages);
            }

            lock (CallbackLock)
            {
                foreach (var onErrorCallback in Configuration.GetOnErrorCallbacks())
                {
                    try
                    {
                        if (!onErrorCallback.Invoke(@event))
                        {
                            return;
                        }
                    }
                    catch (System.Exception)
                    {
                    }
                }
            }

            try
            {
                if (callback != null)
                {
                    if (!callback.Invoke(@event))
                    {
                        return;
                    }
                }
            }
            catch (System.Exception)
            {
            }

            @event.PreparePayload();

            var report = new Report(Configuration, @event);

            if (!report.Ignored)
            {
                Send(report);

                Breadcrumbs.Leave(Breadcrumb.FromReport(report));

                SessionTracking.AddException(report);
            }
        }

        private void RedactMetadata(Metadata metadata)
        {
            foreach (var section in metadata.Payload)
            {
                var theSection = (Dictionary<string, object>)section.Value;
                foreach (var key in theSection.Keys.ToList())
                {
                    if (Configuration.KeyIsRedacted(key))
                    {
                        theSection[key] = "[REDACTED]";
                    }
                }
            }
        }

        private bool ShouldAddProjectPackagesToEvent(Payload.Event theEvent)
        {
            return Application.platform.Equals(RuntimePlatform.Android)
               && Configuration.ProjectPackages != null
               && Configuration.ProjectPackages.Length > 0
               && theEvent.IsAndroidJavaError();
        }

        private bool EventContainsDiscardedClass(Exception[] exceptions)
        {
            foreach (var exception in exceptions)
            {
                if (Configuration.ErrorClassIsDiscarded(exception.ErrorClass))
                {
                    return true;
                }
            }
            return false;
        }

        public void SetApplicationState(bool inFocus)
        {
            if (inFocus)
            {
                _foregroundStopwatch.Start();
                lock (autoSessionLock)
                {
                    if (Configuration.AutoTrackSessions
                     && _backgroundStopwatch.Elapsed.TotalSeconds > AutoCaptureSessionThresholdSeconds)
                    {
                        if (IsUsingFallback())
                        {
                            SessionTracking.StartSession();
                        }
                        else
                        {
                            // The android sdk is unable to listen to the unity activity lifecycle
                            if (Application.platform.Equals(RuntimePlatform.Android))
                            {
                                SessionTracking.ResumeSession();
                            }
                        }
                    }
                    _backgroundStopwatch.Reset();
                }
            }
            else
            {
                _foregroundStopwatch.Stop();
                _backgroundStopwatch.Start();
            }
        }

        /// <summary>
        /// True if reports and sessions should be sent based on release stage settings
        /// </summary>
        private bool ShouldSendRequests()
        {
            return Configuration.ReleaseStage == null
                || Configuration.EnabledReleaseStages == null
                || Configuration.EnabledReleaseStages.Contains(Configuration.ReleaseStage);
        }

        /// <summary>
        /// Check next frame if a new session should be captured
        /// </summary>
        private IEnumerator<UnityEngine.AsyncOperation> RunInitialSessionCheck()
        {
            yield return null;
            if (IsUsingFallback() && Configuration.AutoTrackSessions && SessionTracking.CurrentSession == null)
            {
                SessionTracking.StartSession();
            }
        }

        public void SetContext(string context)
        {
            _contextSetManually = true;

            // set the context property on Configuration, as it currently holds the global state
            Configuration.Context = context;

            // propagate the change to the native property also
            NativeClient.SetContext(context);
        }

        public string GetContext()
        {
            return Configuration.Context;
        }      

        public void MarkLaunchCompleted() => NativeClient.MarkLaunchCompleted();
        
        public void AddOnError(OnErrorCallback bugsnagCallback) => Configuration.AddOnError(bugsnagCallback);

        public void RemoveOnError(OnErrorCallback bugsnagCallback) => Configuration.RemoveOnError(bugsnagCallback);     

        public void AddOnSession(OnSessionCallback callback) => Configuration.AddOnSession(callback);

        public void RemoveOnSession(OnSessionCallback callback) => Configuration.RemoveOnSession(callback);

        public void AddMetadata(string section, string key, object value) => _storedMetadata.AddMetadata(section, key, value);

        public void AddMetadata(string section, Dictionary<string, object> metadata) => _storedMetadata.AddMetadata(section,metadata);

        public void ClearMetadata(string section) => _storedMetadata.ClearMetadata(section);

        public void ClearMetadata(string section, string key) => _storedMetadata.ClearMetadata(section, key);

        public Dictionary<string,object> GetMetadata(string section) => _storedMetadata.GetMetadata(section);

        public object GetMetadata(string section, string key) => _storedMetadata.GetMetadata(section, key);

        public User GetUser()
        {
            return _cachedUser;
        }

        public void SetUser(string id, string email, string name)
        {
            _cachedUser = new User(id, name, email);
        }
    }
}
