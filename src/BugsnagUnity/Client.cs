using BugsnagUnity.Payload;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

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


        public Client(INativeClient nativeClient)
        {
            NativeClient = nativeClient;
            SessionTracking = new SessionTracker(this);
            MainThread = Thread.CurrentThread;
            InitStopwatches();
            InitUserObject();
            InitMetadata();
            InitCounters();
            ListenForSceneLoad();
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
                Breadcrumbs.Leave("Bugsnag loaded", null, BreadcrumbType.State);
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

        private void ListenForSceneLoad()
        {
            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode loadSceneMode) => {
                if (Configuration.IsBreadcrumbTypeEnabled(BreadcrumbType.Navigation))
                {
                    Breadcrumbs.Leave("Scene Loaded", new Dictionary<string, object> { { "sceneName", scene.name } }, BreadcrumbType.Navigation);
                }
                _storedMetadata.AddMetadata("app", "lastLoadedUnityScene",scene.name);
            };
        }       

        public void Send(IPayload payload)
        {
            if (!ShouldSendRequests())
            {
                return;
            }
            Delivery.Send(payload);
        }

        void MultiThreadedNotify(string condition, string stackTrace, LogType logType)
        {
            // Discard messages from the main thread as they will be reported separately
            if (!ReferenceEquals(Thread.CurrentThread, MainThread))
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
            if (!Configuration.EnabledErrorTypes.UnityLog)
            {
                return;
            }
            if (Configuration.AutoDetectErrors && logType.IsGreaterThanOrEqualTo(Configuration.NotifyLogLevel))
            {
                var logMessage = new UnityLogMessage(condition, stackTrace, logType);
                var shouldSend = Error.ShouldSend(logMessage)
                  && _uniqueCounter.ShouldSend(logMessage)
                  && _logTypeCounter.ShouldSend(logMessage);
                if (shouldSend)
                {
                    var severity = Configuration.LogTypeSeverityMapping.Map(logType);
                    var backupStackFrames = new System.Diagnostics.StackTrace(1, true).GetFrames();
                    var forceUnhandled = logType == LogType.Exception && !Configuration.ReportExceptionLogsAsHandled;
                    var exception = Error.FromUnityLogMessage(logMessage, backupStackFrames, severity, forceUnhandled);
                    Notify(new Error[] { exception }, exception.HandledState, null, logType);
                }
            }
            else if (Configuration.ShouldLeaveLogBreadcrumb(logType))
            {
                var metadata = new Dictionary<string, object>()
                {
                    {"logLevel" , logType.ToString() }
                };
                Breadcrumbs.Leave(condition, metadata, BreadcrumbType.Log );
            }
        }

        public void Notify(string name, string message, string stackTrace, Func<IEvent, bool> callback)
        {
            var exceptions = new Error[] { Error.FromStringInfo(name, message, stackTrace) };
            Notify(exceptions, HandledState.ForHandledException(), callback, LogType.Exception);
        }

        public void Notify(System.Exception exception, string stacktrace, Func<IEvent, bool> callback)
        {
            var exceptions = new Errors(exception, stacktrace).ToArray();
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

        public void Notify(System.Exception exception, Func<IEvent, bool> callback)
        {
            Notify(exception, callback, 3);
        }

        internal void Notify(System.Exception exception, Func<IEvent, bool> callback, int level)
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

        public void Notify(System.Exception exception, Severity severity, Func<IEvent, bool> callback)
        {
            Notify(exception, severity, callback, 3);
        }

        internal void Notify(System.Exception exception, Severity severity, Func<IEvent, bool> callback, int level)
        {
            Notify(exception, HandledState.ForUserSpecifiedSeverity(severity), callback, level);
        }

        void Notify(System.Exception exception, HandledState handledState, Func<IEvent, bool> callback, int level)
        {
            // we need to generate a substitute stacktrace here as if we are not able
            // to generate one from the exception that we are given then we are not able
            // to do this inside of the IEnumerator generator code
            var substitute = new System.Diagnostics.StackTrace(level, true).GetFrames();
            Notify(new Errors(exception, substitute).ToArray(), handledState, callback, null);
        }

        void Notify(Error[] exceptions, HandledState handledState, Func<IEvent, bool> callback, LogType? logType)
        {
            if (!ShouldSendRequests() || EventContainsDiscardedClass(exceptions) || !Configuration.Endpoints.IsValid)
            {
                return;
            }


            if (!object.ReferenceEquals(Thread.CurrentThread, MainThread))
            {
                try
                {
                    var asyncHandler = MainThreadDispatchBehaviour.Instance();
                    asyncHandler.Enqueue(() => { NotifyOnMainThread(exceptions, handledState, callback, logType); });
                }
                catch
                {
                    // Async behavior is not available in a test environment
                }
            }
            else
            {
                NotifyOnMainThread(exceptions, handledState, callback, logType);
            }            
           
        }

        private void NotifyOnMainThread(Error[] exceptions, HandledState handledState, Func<IEvent, bool> callback, LogType? logType)
        {
            if (!ShouldSendRequests() || EventContainsDiscardedClass(exceptions) || !Configuration.Endpoints.IsValid)
            {
                return;
            }

            var user = _cachedUser.Clone();

            var app = new AppWithState(Configuration)
            {
                InForeground = InForeground,
                DurationInForeground = _foregroundStopwatch.Elapsed,
            };

            NativeClient.PopulateAppWithState(app);

            var device = new DeviceWithState(Configuration);

            NativeClient.PopulateDeviceWithState(device);

            var eventMetadata = new Metadata();

            eventMetadata.MergeMetadata(NativeClient.GetNativeMetadata());

            AutomaticDataCollector.AddStatefulDeviceData(eventMetadata);

            eventMetadata.MergeMetadata(_storedMetadata.Payload);

            var @event = new Payload.Event(
              Configuration.Context,
              eventMetadata,
              app,
              device,
              user,
              exceptions,
              handledState,
              Breadcrumbs.Retrieve(),
              SessionTracking.CurrentSession,
              Configuration.ApiKey);

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
                    catch
                    {
                        // If the callback causes an exception, ignore it and execute the next one
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
            catch
            {
                // If the callback causes an exception, ignore it and execute the next one
            }


            lock (CallbackLock)
            {
                foreach (var onSendErrorCallback in Configuration.GetOnSendErrorCallbacks())
                {
                    try
                    {
                        if (!onSendErrorCallback.Invoke(@event))
                        {
                            return;
                        }
                    }
                    catch
                    {
                        // If the callback causes an exception, ignore it and execute the next one
                    }
                }
            }

            @event.RedactMetadata(Configuration);

            var report = new Report(Configuration, @event);

            if (!report.Ignored)
            {
                Send(report);
                if (Configuration.IsBreadcrumbTypeEnabled(BreadcrumbType.Error))
                {
                    Breadcrumbs.Leave(Breadcrumb.FromReport(report));
                }
                SessionTracking.AddException(report);
            }
        }

       
        private bool ShouldAddProjectPackagesToEvent(Payload.Event theEvent)
        {
            return Application.platform.Equals(RuntimePlatform.Android)
               && Configuration.ProjectPackages != null
               && Configuration.ProjectPackages.Length > 0
               && theEvent.IsAndroidJavaError();
        }

        private bool EventContainsDiscardedClass(Error[] exceptions)
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
        
        public void AddOnError(Func<IEvent, bool> bugsnagCallback) => Configuration.AddOnError(bugsnagCallback);

        public void RemoveOnError(Func<IEvent, bool> bugsnagCallback) => Configuration.RemoveOnError(bugsnagCallback);

        public void AddOnSession(Func<ISession, bool> callback) => Configuration.AddOnSession(callback);

        public void RemoveOnSession(Func<ISession, bool> callback) => Configuration.RemoveOnSession(callback);

        public void AddMetadata(string section, string key, object value) => _storedMetadata.AddMetadata(section, key, value);

        public void AddMetadata(string section, IDictionary<string, object> metadata) => _storedMetadata.AddMetadata(section,metadata);

        public void ClearMetadata(string section) => _storedMetadata.ClearMetadata(section);

        public void ClearMetadata(string section, string key) => _storedMetadata.ClearMetadata(section, key);

        public IDictionary<string,object> GetMetadata(string section) => _storedMetadata.GetMetadata(section);

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
