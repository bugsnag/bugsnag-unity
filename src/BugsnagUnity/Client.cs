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
        public IConfiguration Configuration => NativeClient.Configuration;

        public IBreadcrumbs Breadcrumbs => NativeClient.Breadcrumbs;

        public ISessionTracker SessionTracking { get; }

        public User User { get; }

        public Metadata Metadata { get; }

        UniqueLogThrottle UniqueCounter { get; }

        MaximumLogTypeCounter LogTypeCounter { get; }

        protected IDelivery Delivery => NativeClient.Delivery;

        List<Middleware> Middleware { get; }

        object MiddlewareLock { get; } = new object();

        internal INativeClient NativeClient { get; }

        Stopwatch ForegroundStopwatch { get; }

        Stopwatch BackgroundStopwatch { get; }

        bool InForeground => ForegroundStopwatch.IsRunning;

        const string AppMetadataKey = "app";

        const string DeviceMetadataKey = "device";

        private Thread MainThread;

        private static double AutoCaptureSessionThresholdSeconds = 30;

        private GameObject TimingTrackerObject { get; }

        private static object autoSessionLock = new object();

        private static bool _contextSetManually;

        public Client(INativeClient nativeClient)
        {
            MainThread = Thread.CurrentThread;
            ForegroundStopwatch = new Stopwatch();
            BackgroundStopwatch = new Stopwatch();
            NativeClient = nativeClient;
            User = new User { Id = SystemInfo.deviceUniqueIdentifier };
            Middleware = new List<Middleware>();
            Metadata = new Metadata(nativeClient);
            UniqueCounter = new UniqueLogThrottle(Configuration);
            LogTypeCounter = new MaximumLogTypeCounter(Configuration);
            SessionTracking = new SessionTracker(this);

            UnityMetadata.InitDefaultMetadata();
            NativeClient.SetMetadata(AppMetadataKey, UnityMetadata.DefaultAppMetadata);
            NativeClient.SetMetadata(DeviceMetadataKey, UnityMetadata.DefaultDeviceMetadata);

            Device.InitUnityVersion();
            NativeClient.PopulateUser(User);
            if (!string.IsNullOrEmpty(nativeClient.Configuration.Context))
            {
                _contextSetManually = true;
            }

            SetupSceneLoadedBreadcrumbTracking();

            Application.logMessageReceivedThreaded += MultiThreadedNotify;
            Application.logMessageReceived += Notify;
            User.PropertyChanged += (obj, args) => { NativeClient.SetUser(User); };
            TimingTrackerObject = new GameObject("Bugsnag app lifecycle tracker");
            TimingTrackerObject.AddComponent<TimingTrackerBehaviour>();
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
            if (!Configuration.Endpoints.IsValid)
            {
                CheckForMisconfiguredEndpointsWarning();
            }
            AddBugsnagLoadedBreadcrumb();
        }

        private void CheckForMisconfiguredEndpointsWarning()
        {
            var endpoints = Configuration.Endpoints;
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

        /// <summary>
        /// Sets the current context to the scene name and leaves a breadcrumb with
        /// the current scene information.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="loadSceneMode"></param>
        void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (!_contextSetManually)
            {
                Configuration.Context = scene.name;

                // propagate the change to the native property also
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
                  && UniqueCounter.ShouldSend(logMessage)
                  && LogTypeCounter.ShouldSend(logMessage);
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



        public void BeforeNotify(Middleware middleware)
        {
            lock (MiddlewareLock)
            {
                Middleware.Add(middleware);
            }
        }

        public void Notify(System.Exception exception)
        {
            Notify(exception, 3);
        }

        internal void Notify(System.Exception exception, int level)
        {
            Notify(exception, HandledState.ForHandledException(), null, level);
        }

        public void Notify(System.Exception exception, Middleware callback)
        {
            Notify(exception, callback, 3);
        }

        internal void Notify(System.Exception exception, Middleware callback, int level)
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

        public void Notify(System.Exception exception, Severity severity, Middleware callback)
        {
            Notify(exception, severity, callback, 3);
        }

        internal void Notify(System.Exception exception, Severity severity, Middleware callback, int level)
        {
            Notify(exception, HandledState.ForUserSpecifiedSeverity(severity), callback, level);
        }

        void Notify(System.Exception exception, HandledState handledState, Middleware callback, int level)
        {
            // we need to generate a substitute stacktrace here as if we are not able
            // to generate one from the exception that we are given then we are not able
            // to do this inside of the IEnumerator generator code
            var substitute = new System.Diagnostics.StackTrace(level, true).GetFrames();
            Notify(new Exceptions(exception, substitute).ToArray(), handledState, callback, null);
        }

        void Notify(Exception[] exceptions, HandledState handledState, Middleware callback, LogType? logType)
        {
            if (!ShouldSendRequests() || EventContainsDiscardedClass(exceptions) || !Configuration.Endpoints.IsValid)
            {
                return; // Skip overhead of computing payload to to ultimately not be sent
            }

            var user = new User { Id = User.Id, Email = User.Email, Name = User.Name };
            var app = new App(Configuration)
            {
                InForeground = InForeground,
                DurationInForeground = ForegroundStopwatch.Elapsed,
            };
            NativeClient.PopulateApp(app);
            var device = new Device();
            NativeClient.PopulateDevice(device);
            device.AddRuntimeVersions(Configuration);

            var metadata = new Metadata();
            NativeClient.PopulateMetadata(metadata);

            foreach (var item in Metadata)
            {
                metadata.AddToPayload(item.Key, item.Value);
            }

            RedactMetadata(metadata);

            var session = IsUsingFallback() ? SessionTracking.CurrentSession : NativeClient.GetCurrentSession();

            var @event = new Payload.Event(
              Configuration.Context,
              metadata,
              app,
              device,
              user,
              exceptions,
              handledState,
              Breadcrumbs.Retrieve(),
              session);

            //Check for adding project packages to an android java error event
            if (ShouldAddProjectPackagesToEvent(@event))
            {
                @event.AddAndroidProjectPackagesToEvent(Configuration.ProjectPackages);
            }

            var report = new Report(Configuration, @event);

            lock (MiddlewareLock)
            {
                foreach (var middleware in Middleware)
                {
                    try
                    {
                        middleware(report);
                    }
                    catch (System.Exception)
                    {
                    }
                }
            }

            try
            {
                callback?.Invoke(report);
            }
            catch (System.Exception)
            {
            }

            if (!report.Ignored)
            {
                Send(report);

                Breadcrumbs.Leave(Breadcrumb.FromReport(report));

                SessionTracking.AddException(report);
            }
        }

        private void RedactMetadata(Metadata metadata)
        {
            foreach (var section in metadata)
            {
                var sectionDictionaryType = section.Value.GetType();
                if (sectionDictionaryType == typeof(Dictionary<string, object>))
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
                if (sectionDictionaryType == typeof(Dictionary<string, string>))
                {
                    var theSection = (Dictionary<string, string>)section.Value;
                    foreach (var key in theSection.Keys.ToList())
                    {
                        if (Configuration.KeyIsRedacted(key))
                        {
                            theSection[key] = "[REDACTED]";
                        }
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
                ForegroundStopwatch.Start();
                lock (autoSessionLock)
                {
                    if (Configuration.AutoTrackSessions
                     && BackgroundStopwatch.Elapsed.TotalSeconds > AutoCaptureSessionThresholdSeconds
                     && IsUsingFallback())
                    {
                        SessionTracking.StartSession();
                    }
                    BackgroundStopwatch.Reset();
                }
            }
            else
            {
                ForegroundStopwatch.Stop();
                BackgroundStopwatch.Start();
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

        public void SetAutoDetectErrors(bool autoDetectErrors)
        {
            // set the property on Configuration, as it currently controls whether C# errors are reported
            Configuration.AutoDetectErrors = autoDetectErrors;

            // propagate the change to the native property also
            NativeClient.SetAutoDetectErrors(autoDetectErrors);
        }

        public void SetAutoDetectAnrs(bool autoDetectAnrs)
        {
            // Set the native property
            NativeClient.SetAutoDetectAnrs(autoDetectAnrs);
        }
    }
}
