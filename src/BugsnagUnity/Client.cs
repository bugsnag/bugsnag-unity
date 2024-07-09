using BugsnagUnity.Payload;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using BugsnagNetworking;
using UnityEngine.Networking;

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

        internal CacheManager CacheManager;

        internal PayloadManager PayloadManager;

        private Delivery _delivery;

        object CallbackLock { get; } = new object();

        internal INativeClient NativeClient { get; }

        private Stopwatch _foregroundStopwatch;

        private Stopwatch _backgroundStopwatch;

        bool InForeground => _foregroundStopwatch.IsRunning;

        private Thread MainThread;

        private static double AutoCaptureSessionThresholdSeconds = 30;

        private static object autoSessionLock = new object();

        private OrderedDictionary _featureFlags;

        private bool _isUnity2019OrHigher;

        private class BugsnagLogHandler : ILogHandler
        {

            private ILogHandler _oldLogHandler;

            private Client _client;

            private Configuration _config => _client.Configuration;

            public BugsnagLogHandler(ILogHandler oldLogHandler, Client client)
            {
                _oldLogHandler = oldLogHandler;
                _client = client;
            }

            public void LogException(System.Exception exception, UnityEngine.Object context)
            {
                if (exception == null)
                {
                    return;
                }
                if (_config.AutoDetectErrors && LogType.Exception.IsGreaterThanOrEqualTo(_config.NotifyLogLevel))
                {
                    var unityLogMessage = new UnityLogMessage(exception);
                    var shouldSend = Error.ShouldSend(exception)
                      && _client._uniqueCounter.ShouldSend(unityLogMessage)
                      && _client._logTypeCounter.ShouldSend(unityLogMessage);
                    if (shouldSend)
                    {
                        var handledState = _config.ReportExceptionLogsAsHandled ? HandledState.ForLoggedException() : HandledState.ForUnhandledException();
                        _client.Notify(exception, handledState, null);
                    }
                }
                if (_oldLogHandler != null)
                {
                    _oldLogHandler.LogException(exception, context);
                }
            }

            public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
            {
                if (_oldLogHandler != null)
                {
                    _oldLogHandler.LogFormat(logType, context, format, args);
                }
            }
        }

        private void SetupAdvancedExceptionInterceptor()
        {
            var oldHandler = UnityEngine.Debug.unityLogger.logHandler;
            UnityEngine.Debug.unityLogger.logHandler = new BugsnagLogHandler(oldHandler, this);
        }

        public Client(INativeClient nativeClient)
        {
            InitMainthreadDispatcher();
            NativeClient = nativeClient;
            CacheManager = new CacheManager(Configuration);
            PayloadManager = new PayloadManager(CacheManager);
            _delivery = new Delivery(this, Configuration, CacheManager, PayloadManager);
            MainThread = Thread.CurrentThread;
            SessionTracking = new SessionTracker(this);
            _isUnity2019OrHigher = IsUnity2019OrHigher();
            InitStopwatches();
            InitUserObject();
            InitMetadata();
            InitFeatureFlags();
            InitCounters();
            if (_isUnity2019OrHigher)
            {
                SetupAdvancedExceptionInterceptor();
            }
            InitTimingTracker();
            StartInitialSession();
            CheckForMisconfiguredEndpointsWarning();
            AddBugsnagLoadedBreadcrumb();
            _delivery.StartDeliveringCachedPayloads();
            ListenForSceneLoad();
            SetupNetworkListeners();
            InitLogHandlers();
        }

        private void InitMainthreadDispatcher()
        {
            MainThreadDispatchBehaviour.Instance();
        }

        private bool IsUnity2019OrHigher()
        {
            var version = Application.unityVersion;
            //will be null in unit tests
            if (version == null)
            {
                return false;
            }
            return !version.Contains("2017") &&
                !version.Contains("2018");
        }

        private void InitFeatureFlags()
        {
            if (Configuration.FeatureFlags != null)
            {
                _featureFlags = Configuration.FeatureFlags;
            }
            else
            {
                _featureFlags = new OrderedDictionary();
            }
        }

        private void StartInitialSession()
        {
            if (IsUsingFallback() && Configuration.AutoTrackSessions && SessionTracking.CurrentSession == null)
            {
                SessionTracking.StartSession();
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
            Application.logMessageReceived += NotifyFromUnityLog;
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
            // Required in case the focus event is not recieved (if Bugsnag is started after it is sent)
            _foregroundStopwatch.Start();
        }

        private void InitUserObject()
        {
            if (Configuration.GetUser() != null)
            {
                // if a user is supplied in the config then use that
                _cachedUser = Configuration.GetUser();
            }
            else
            {
                // otherwise create one
                _cachedUser = new User { Id = CacheManager.GetCachedDeviceId() };
                // see if a native user is avaliable
                NativeClient.PopulateUser(_cachedUser);
            }
            // listen for changes and pass the data to the native layer
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
            return Application.platform != RuntimePlatform.Android &&
                Application.platform != RuntimePlatform.OSXPlayer &&
                Application.platform != RuntimePlatform.IPhonePlayer;
        }

        private void ListenForSceneLoad()
        {
            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode loadSceneMode) =>
            {
                if (Configuration.IsBreadcrumbTypeEnabled(BreadcrumbType.Navigation))
                {
                    Breadcrumbs.Leave("Scene Loaded", new Dictionary<string, object> { { "sceneName", scene.name } }, BreadcrumbType.Navigation);
                }
                _storedMetadata.AddMetadata("app", "lastLoadedUnityScene", scene.name);
            };
        }

        public void Send(IPayload payload)
        {
            if (!ShouldSendRequests())
            {
                return;
            }
            _delivery.Deliver(payload);
        }

        void MultiThreadedNotify(string condition, string stackTrace, LogType logType)
        {
            // Discard messages from the main thread as they will be reported separately
            if (!ReferenceEquals(Thread.CurrentThread, MainThread))
            {
                NotifyFromUnityLog(condition, stackTrace, logType);
            }
        }

        private void NotifyFromUnityLog(string condition, string stackTrace, LogType logType)
        {
            if (!Configuration.EnabledErrorTypes.UnityLog)
            {
                return;
            }
            if (logType.Equals(LogType.Exception) && _isUnity2019OrHigher)
            {
                return;
            }
            if (condition.StartsWith("BUGSNAG_MAZERUNNER_LOG"))
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
                Breadcrumbs.Leave(condition, metadata, BreadcrumbType.Log);
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

        public void Notify(System.Exception exception, Func<IEvent, bool> callback)
        {
            Notify(exception, HandledState.ForHandledException(), callback);
        }

        public void Notify(System.Exception exception, Severity severity, Func<IEvent, bool> callback)
        {
            Notify(exception, HandledState.ForUserSpecifiedSeverity(severity), callback);
        }

        void Notify(System.Exception exception, HandledState handledState, Func<IEvent, bool> callback)
        {
            // we need to generate a substitute stacktrace here as if we are not able
            // to generate one from the exception that we are given then we are not able
            // to do this inside of the IEnumerator generator code
            var substitute = new System.Diagnostics.StackTrace(true).GetFrames();
            var errors = new Errors(exception, substitute).ToArray();
            foreach (var error in errors)
            {
                if (error.IsAndroidJavaException)
                {
                    handledState = HandledState.ForUnhandledException();
                }
            }
            Notify(errors, handledState, callback, null);
        }

        private void Notify(Error[] exceptions, HandledState handledState, Func<IEvent, bool> callback, LogType? logType)
        {
            if (!ShouldSendRequests() || EventContainsDiscardedClass(exceptions) || !Configuration.Endpoints.IsValid)
            {
                return;
            }


            if (!ReferenceEquals(Thread.CurrentThread, MainThread))
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

            var device = new DeviceWithState(Configuration, CacheManager.GetCachedDeviceId());

            NativeClient.PopulateDeviceWithState(device);

            var eventMetadata = new Metadata();

            eventMetadata.MergeMetadata(NativeClient.GetNativeMetadata());

            AutomaticDataCollector.AddStatefulDeviceData(eventMetadata);

            var activeScene = SceneManager.GetActiveScene();
            if (activeScene != null)
            {
                eventMetadata.AddMetadata("app", "activeUnityScene", activeScene.name);
            }

            eventMetadata.MergeMetadata(_storedMetadata.Payload);

            var featureFlags = new OrderedDictionary();
            foreach (DictionaryEntry entry in _featureFlags)
            {
                featureFlags[entry.Key] = entry.Value;
            }

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
              Configuration.ApiKey,
              featureFlags);

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

            var report = new Report(Configuration, @event);
            if (!report.Ignored)
            {
                //if serialisation fails, then we ignore the event
                if (PayloadManager.AddPendingPayload(report))
                {
                    Send(report);
                    if (Configuration.IsBreadcrumbTypeEnabled(BreadcrumbType.Error))
                    {
                        Breadcrumbs.Leave(Breadcrumb.FromReport(report));
                    }
                    SessionTracking.AddException(report);
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
                _delivery.StartDeliveringCachedPayloads();
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

        public void AddOnSession(Func<ISession, bool> callback)
        {
            Configuration.AddOnSession(callback);
            NativeClient.RegisterForOnSessionCallbacks();
        }

        public void RemoveOnSession(Func<ISession, bool> callback) => Configuration.RemoveOnSession(callback);

        public void AddMetadata(string section, string key, object value) => _storedMetadata.AddMetadata(section, key, value);

        public void AddMetadata(string section, IDictionary<string, object> metadata) => _storedMetadata.AddMetadata(section, metadata);

        public void ClearMetadata(string section) => _storedMetadata.ClearMetadata(section);

        public void ClearMetadata(string section, string key) => _storedMetadata.ClearMetadata(section, key);

        public IDictionary<string, object> GetMetadata(string section) => _storedMetadata.GetMetadata(section);

        public object GetMetadata(string section, string key) => _storedMetadata.GetMetadata(section, key);

        public User GetUser()
        {
            return _cachedUser;
        }

        public void SetUser(string id, string email, string name)
        {
            _cachedUser = new User(id, email, name);
            NativeClient.SetUser(_cachedUser);
        }

        public void AddFeatureFlag(string name, string variant = null)
        {
            NativeClient.AddFeatureFlag(name, variant);
            _featureFlags[name] = variant;
        }

        public void AddFeatureFlags(FeatureFlag[] featureFlags)
        {
            foreach (var flag in featureFlags)
            {
                AddFeatureFlag(flag.Name, flag.Variant);
            }
        }

        public void ClearFeatureFlag(string name)
        {
            _featureFlags.Remove(name);
            NativeClient.ClearFeatureFlag(name);
        }

        public void ClearFeatureFlags()
        {
            _featureFlags.Clear();
            NativeClient.ClearFeatureFlags();
        }

        private void SetupNetworkListeners()
        {
            // Currently network breadcrumb are the only feature using the web events. 
            // If we add more features that use web request events, we will need to move this check
            if (!Configuration.IsBreadcrumbTypeEnabled(BreadcrumbType.Request))
            {
                return;
            }
            BugsnagUnityWebRequest.OnSend.AddListener(OnWebRequestSend);
            BugsnagUnityWebRequest.OnComplete.AddListener(OnWebRequestComplete);
            BugsnagUnityWebRequest.OnAbort.AddListener(OnWebRequestAbort);
        }

        private readonly Dictionary<BugsnagUnityWebRequest, DateTimeOffset> _requestStartTimes = new Dictionary<BugsnagUnityWebRequest, DateTimeOffset>();


        private void OnWebRequestComplete(BugsnagUnityWebRequest request)
        {
            if (_requestStartTimes.TryGetValue(request, out DateTimeOffset startTime))
            {
                TimeSpan duration = DateTimeOffset.UtcNow - startTime;
                LeaveNetworkBreadcrumb(request.UnityWebRequest, duration);
                _requestStartTimes.Remove(request);
            }
            else
            {
                LeaveNetworkBreadcrumb(request.UnityWebRequest, null);
            }
        }

        private void OnWebRequestSend(BugsnagUnityWebRequest request)
        {
            _requestStartTimes[request] = DateTimeOffset.UtcNow;
        }

        private void OnWebRequestAbort(BugsnagUnityWebRequest request)
        {
            if (_requestStartTimes.ContainsKey(request))
            {
                _requestStartTimes.Remove(request);
            }
        }

        public void LeaveNetworkBreadcrumb(UnityWebRequest request, TimeSpan? duration)
        {
            string statusMessage = request.result == UnityWebRequest.Result.Success ? "succeeded" : "failed";
            string fullMessage = $"UnityWebRequest {statusMessage}";
            var metadata = new Dictionary<string, object>();
            metadata["status"] = request.responseCode;
            metadata["method"] = request.method;
            metadata["url"] = request.url;
            var urlParams = ExtractUrlParams(request.uri);
            if (urlParams.Count > 0)
            {
                metadata["urlParams"] = urlParams;
            }
            metadata["duration"] = duration?.TotalMilliseconds;
            if (request.uploadHandler != null && request.uploadHandler.data != null)
            {
                metadata["requestContentLength"] = request.uploadHandler.data.Length;
            }
            if (request.downloadHandler != null && request.downloadHandler.data != null)
            {
                metadata["responseContentLength"] = request.downloadHandler.data.Length;
            }
            Breadcrumbs.Leave(fullMessage, metadata, BreadcrumbType.Request);
        }

        private Dictionary<string, string> ExtractUrlParams(Uri uri)
        {
            var queryParams = new Dictionary<string, string>();
            var querySegments = uri.Query.TrimStart('?').Split('&');

            foreach (var segment in querySegments)
            {
                var parts = segment.Split('=');
                if (parts.Length == 2)
                {
                    if (Configuration.KeyIsRedacted(parts[0]))
                    {
                        queryParams[parts[0]] = "[REDACTED]";
                    }
                    else
                    {
                        queryParams[parts[0]] = parts[1];
                    }
                }
            }

            return queryParams;
        }


    }
}
