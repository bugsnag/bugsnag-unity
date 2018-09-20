#if UNITY_5_3_OR_NEWER || UNITY_5
#define UNITY_5_OR_NEWER
#endif

using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using System.Text.RegularExpressions;
#if UNITY_5_OR_NEWER
using UnityEngine.SceneManagement;
#endif

public class Bugsnag : MonoBehaviour {

#if UNITY_STANDALONE_OSX
    private const string dllName = "bugsnag-osx";
#else
    private const string dllName = "__Internal";
#endif

    public class NativeBugsnag {
        public static void Register(string apiKey) {
            Register(apiKey, false);
        }
#if (UNITY_IPHONE || UNITY_IOS || UNITY_TVOS || UNITY_WEBGL || UNITY_STANDALONE_OSX) && !UNITY_EDITOR
        [DllImport (dllName, EntryPoint = "BSGRegister")]
        public static extern void Register(string apiKey, bool trackSessions);

        [DllImport (dllName, EntryPoint = "BSGNotify")]
        public static extern void Notify(string errorClass, string errorMessage, string severity, string context, string stackTrace, string type, string severityReason);

        [DllImport (dllName, EntryPoint = "BSGSetNotifyUrl")]
        public static extern void SetNotifyUrl(string notifyUrl);

        [DllImport (dllName, EntryPoint = "BSGSetAutoNotify")]
        public static extern void SetAutoNotify(bool autoNotify);

        [DllImport (dllName, EntryPoint = "BSGSetContext")]
        public static extern void SetContext(string context);

        [DllImport (dllName, EntryPoint = "BSGSetReleaseStage")]
        public static extern void SetReleaseStage(string releaseStage);

        [DllImport (dllName, EntryPoint = "BSGSetNotifyReleaseStages")]
        public static extern void SetNotifyReleaseStages(string releaseStages);

        [DllImport (dllName, EntryPoint = "BSGAddToTab")]
        public static extern void AddToTab(string tabName, string attributeName, string attributeValue);

        [DllImport (dllName, EntryPoint = "BSGClearTab")]
        public static extern void ClearTab(string tabName);

        [DllImport (dllName, EntryPoint = "BSGLeaveBreadcrumb")]
        public static extern void LeaveBreadcrumb(string breadcrumb);

        [DllImport (dllName, EntryPoint = "BSGSetBreadcrumbCapacity")]
        public static extern void SetBreadcrumbCapacity(int capacity);

        [DllImport (dllName, EntryPoint = "BSGSetAppVersion")]
        public static extern void SetAppVersion(string appVersion);

        [DllImport (dllName, EntryPoint = "BSGSetUser")]
        public static extern void SetUser(string userId, string userName, string userEmail);

        [DllImport (dllName, EntryPoint = "BSGStartSession")]
        public static extern void StartSession();

        [DllImport (dllName, EntryPoint = "BSGSetSessionUrl")]
        public static extern void SetSessionUrl(string sessionUrl);

#elif UNITY_ANDROID && !UNITY_EDITOR
        public static AndroidJavaClass BugsnagUnity = new AndroidJavaClass("com.bugsnag.android.unity.UnityClient");
        public static Regex unityExpression = new Regex ("(\\S+)\\s*\\(.*?\\)\\s*(?:(?:\\[.*\\]\\s*in\\s|\\(at\\s*\\s*)(.*):(\\d+))?", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public static void Register(string apiKey, bool trackSessions) {
            // Get the current Activity
            AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject app = activity.Call<AndroidJavaObject>("getApplicationContext");

            jvalue[] args =  new jvalue[3] {
                new jvalue() { l = app.GetRawObject() },
                new jvalue() { l = AndroidJNI.NewStringUTF(apiKey) },
                new jvalue() { l = (IntPtr)(trackSessions ? 1 : 0) },
            };
            IntPtr methodId = AndroidJNI.GetStaticMethodID(BugsnagUnity.GetRawClass(), "init", "(Landroid/content/Context;Ljava/lang/String;Z)V");
            AndroidJNI.CallStaticVoidMethod(BugsnagUnity.GetRawClass(), methodId, args);
            registered_ = true;
            Notify("errorClass", "error message", "error", "", new System.Diagnostics.StackTrace (1, true).ToString (), null, true, "");
        }

		public static void Notify(string errorClass, string errorMessage, string severity, string context, string stackTrace, string type, string severityReason) {
            Notify(errorClass, errorMessage, severity, context, stackTrace, type, false, severityReason);
        }

        public static void Notify(string errorClass, string errorMessage, string severity, string context, string stackTrace, string type, bool warmup, string severityReason) {
            if (!CheckRegistration()) return;
            var stackFrames = new ArrayList ();

            foreach (Match frameMatch in unityExpression.Matches(stackTrace)) {

                var method = frameMatch.Groups[1].Value;
                var className = "";
                if (method == "") {
                    method = "Unknown method";
                } else {
                    var index = method.LastIndexOf(".");
                    if (index >= 0) {
                        className = method.Substring (0, index);
                        method = method.Substring (index + 1);
                    }
                }
                var file = frameMatch.Groups[2].Value;
                if (file == "" || file == "<filename unknown>") {
                    file = "unknown file";
                }
                var line = frameMatch.Groups[3].Value;
                if (line == "") {
                    line = "0";
                }

                var stackFrame = new AndroidJavaObject("java.lang.StackTraceElement", className, method, file, Convert.ToInt32(line));
                    stackFrames.Add (stackFrame);
            }

            if (stackFrames.Count > 0 && warmup == false) {

                IntPtr stackFrameArrayObject  = AndroidJNI.NewObjectArray(stackFrames.Count, ((AndroidJavaObject)(stackFrames[0])).GetRawClass(), ((AndroidJavaObject)(stackFrames[0])).GetRawObject());

                for (var i = 1; i < stackFrames.Count; i++) {
                    AndroidJNI.SetObjectArrayElement(stackFrameArrayObject, i, ((AndroidJavaObject)(stackFrames[i])).GetRawObject());
                }

                var Severity = new AndroidJavaClass("com.bugsnag.android.Severity");
                var severityInstance = Severity.GetStatic<AndroidJavaObject>("ERROR");

                if (severity == "info") {
                    severityInstance = Severity.GetStatic<AndroidJavaObject>("INFO");
                } else if (severity == "warning") {
                    severityInstance = Severity.GetStatic<AndroidJavaObject>("WARNING");
                }

                // Build the arguments
                jvalue[] args =  new jvalue[7] {
                    new jvalue() { l = AndroidJNI.NewStringUTF(errorClass) },
                    new jvalue() { l = AndroidJNI.NewStringUTF(errorMessage) },
                    new jvalue() { l = AndroidJNI.NewStringUTF(context) },
                    new jvalue() { l = (IntPtr)stackFrameArrayObject },
                    new jvalue() { l = severityInstance.GetRawObject() },
                    new jvalue() { l = String.IsNullOrEmpty(type) ? IntPtr.Zero : AndroidJNI.NewStringUTF(type) },
					new jvalue() { l = AndroidJNI.NewStringUTF(String.IsNullOrEmpty(severityReason) ? "handledException" : severityReason) }
                };

                // Call Android's notify method
                IntPtr clientConstructorId = AndroidJNI.GetStaticMethodID(BugsnagUnity.GetRawClass(), "notify", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;[Ljava/lang/StackTraceElement;Lcom/bugsnag/android/Severity;Ljava/lang/String;Ljava/lang/String;)V");
                if(warmup == false) AndroidJNI.CallStaticVoidMethod(BugsnagUnity.GetRawClass(), clientConstructorId, args);
            }
        }

        public static void SetNotifyUrl(string notifyUrl) {
            if (!CheckRegistration()) return;
            BugsnagUnity.CallStatic ("setEndpoint", notifyUrl);
        }

        public static void SetSessionUrl(string url) {
            if (!CheckRegistration()) return;
            BugsnagUnity.CallStatic ("setSessionEndpoint", url);
        }

        public static void SetAutoNotify(bool autoNotify) {
            if (!CheckRegistration()) return;
            BugsnagUnity.CallStatic ("setAutoNotify", autoNotify);
        }

        public static void SetContext(string context) {
            if (!CheckRegistration()) return;
            BugsnagUnity.CallStatic ("setContext", context);
        }

        public static void SetReleaseStage(string releaseStage) {
            // bypass calling CheckRegistration method here as we don't
            // want to log an error as this can now be called prior to
            // setting up the notifier
            if (!registered_) return;
            BugsnagUnity.CallStatic ("setReleaseStage", releaseStage);
        }

        public static void SetNotifyReleaseStages(string releaseStages) {
            if (!CheckRegistration()) return;
            BugsnagUnity.CallStatic ("setNotifyReleaseStages", releaseStages.Split (','));
        }

        public static void AddToTab(string tabName, string attributeName, string attributeValue) {
            if (!CheckRegistration()) return;
            BugsnagUnity.CallStatic ("addToTab", tabName, attributeName, attributeValue);
        }

        public static void ClearTab(string tabName) {
            if (!CheckRegistration()) return;
            BugsnagUnity.CallStatic ("clearTab", tabName);
        }

        public static void LeaveBreadcrumb(string breadcrumb) {
            if (!CheckRegistration()) return;
            BugsnagUnity.CallStatic ("leaveBreadcrumb", breadcrumb);
        }

        public static void SetBreadcrumbCapacity(int capacity) {
            if (!CheckRegistration()) return;
            BugsnagUnity.CallStatic ("setMaxBreadcrumbs", capacity);
        }

        public static void SetAppVersion(string version) {
            if (!CheckRegistration()) return;
            BugsnagUnity.CallStatic ("setAppVersion", version);
        }

        public static void SetUser(string userId, string userName, string userEmail) {
            if (!CheckRegistration()) return;
            BugsnagUnity.CallStatic ("setUser", userId, userEmail, userName);
        }

        public static void StartSession() {
            if (!CheckRegistration()) return;
            BugsnagUnity.CallStatic ("startSession");
        }
#else
        private static string apiKey_;

        public static void SetContext(string context) {}
        public static void SetReleaseStage(string releaseStage) {}
        public static void SetNotifyReleaseStages(string releaseStages) {}
		public static void Notify(string errorClass, string errorMessage, string severity, string context, string stackTrace, string type, string severityReason) {
            if (apiKey_ == null || apiKey_ == "") {
                Debug.Log("BUGSNAG: ERROR: would not notify Bugsnag as no API key was set");
            } else {
                Debug.Log("BUGSNAG: Would notify Bugsnag about " + errorClass + ": " + errorMessage);
            }
        }
        public static void Register(string apiKey, bool trackSession) {
            apiKey_ = apiKey;
        }
        public static void SetNotifyUrl(string notifyUrl) {}
        public static void SetSessionUrl(string sessionUrl) {}
        public static void SetAutoNotify(bool autoNotify) {}
        public static void AddToTab(string tabName, string attributeName, string attributeValue) {}
        public static void ClearTab(string tabName) {}
        public static void LeaveBreadcrumb(string breadcrumb) {}
        public static void SetBreadcrumbCapacity(int capacity) {}
        public static void SetAppVersion(string version) {}
        public static void SetUser(string userId, string userName, string userEmail) {}
        public static void StartSession() {}
#endif
    }
    // We dont use the LogType enum in Unity as the numerical order doesnt suit our purposes
    public enum LogSeverity {
        Log = 0,
        Warning = 1,
        Assert = 2,
        Error = 3,
        Exception = 4
    }

    // Defines the available severities in Bugsnag
    public enum Severity {
        Info = 0,
        Error = 1,
        Warning = 2
    }

    private static volatile bool registered_ = false;
    private static bool CheckRegistration() {
        if (!registered_) {
            Debug.LogError("BUGSNAG: ERROR: Bugsnag must be initialized before calling other methods");
            return false;
        }
        return true;
    }

    // Defines a translation between the Unity log types and Bugsnag log types with appropriate ordering
    private static Dictionary<LogType, LogSeverity> logTypeMapping = new Dictionary<LogType, LogSeverity>()
    {
        { LogType.Assert, LogSeverity.Assert },
        { LogType.Error, LogSeverity.Error },
        { LogType.Exception, LogSeverity.Exception },
        { LogType.Log, LogSeverity.Log },
        { LogType.Warning, LogSeverity.Warning }
    };

    // Defines a default mapping between Unity severities and Bugsnag severities
    private static Dictionary<LogSeverity, Severity> defaultMapping = new Dictionary<LogSeverity, Severity>()
    {
        { LogSeverity.Assert, Severity.Warning },
        { LogSeverity.Error, Severity.Warning },
        { LogSeverity.Exception, Severity.Error },
        { LogSeverity.Log, Severity.Info },
        { LogSeverity.Warning, Severity.Warning }
    };

    // Defines a custom mapping between Unity severities and Bugsnag severities
    private static Dictionary<LogSeverity,Severity> customMapping = new Dictionary<LogSeverity,Severity>();

    // Defines the strings used for the severities
    public static string[] SeverityValues = new string[]{"info", "error", "warning"};

    public string BugsnagApiKey = "";
    public static string BugsnagApiKeyStatic = "";
    public bool AutoNotify = true;
    public bool TrackAppSessions = false;
    public static bool TrackAppSessionsStatic = false;
    private static string BugsnagReleaseStageStatic = null;

    // Rate limiting section
    // Defines the maximum number of logs to send (per type) in the rate limit time frame
    private static Dictionary<LogType, int> maxCounts = new Dictionary<LogType, int>()
    {
      { LogType.Assert, 5 },
      { LogType.Error, 5 },
      { LogType.Exception, 20 },
      { LogType.Log, 5 },
      { LogType.Warning, 5 }
    };

    // Defines the current number of logs send (per type) in the current rate limit time frame
    private static Dictionary<LogType, int> currentCounts = new Dictionary<LogType, int>()
    {
      { LogType.Assert, 0 },
      { LogType.Error, 0 },
      { LogType.Exception, 0 },
      { LogType.Log, 0 },
      { LogType.Warning, 0 }
    };

    // Defines the time period in which the number of maximum logs are sent (default 1 second)
    public static TimeSpan RateLimitTimePeriod = new TimeSpan (0, 0 , 1);

    // Defines the time period in which only unique logs are sent (default 5 seconds)
    public static TimeSpan UniqueLogsTimePeriod = new TimeSpan (0, 0, 5);

    // Defines the maximum number of unique logs we will dedupe, before throwing them away
    private static int maxUniqueLogCount = 500;

    // Contains the latest unique logs in the over the unique log time span.
    private static List<string> latestLogs = new List<string>(maxUniqueLogCount);

    // Defines if we are limiting unity logs (rate limiting and deduping multiple)
    public static bool LimitUnityLogs = true;

    // Allow the notify level to be set for all Bugsnag objects
    public static LogSeverity NotifyLevel = LogSeverity.Exception;

    // Time when the counts were reset last
    private static DateTime timeCountsLastReset = DateTime.UtcNow;

    // Time when the unique logs were reset last
    private static DateTime timeUniqueLogsLastReset = DateTime.UtcNow;

    public static string ReleaseStage {
        set {
            if (value == null) {
                value = "production";
            }
            BugsnagReleaseStageStatic = value;
            NativeBugsnag.SetReleaseStage(value);
        }
    }

    public static string Context {
        set {
            if (value == null) {
                value = "";
            }
            NativeBugsnag.SetContext(value);
        }
    }

    public static string NotifyUrl {
        set {
            if (value == null) {
                value = "https://notify.bugsnag.com/";
            }

            NativeBugsnag.SetNotifyUrl(value);
        }
    }

    public static string SessionUrl {
        set {
            if (value == null) {
                value = "https://sessions.bugsnag.com/";
            }

            NativeBugsnag.SetSessionUrl(value);
        }
    }

    public static string[] NotifyReleaseStages {
        set {
            NativeBugsnag.SetNotifyReleaseStages(String.Join (",", value));
        }
    }

    public static void StartSession() {
        NativeBugsnag.StartSession();
    }

    public static void MapUnityLogToSeverity(LogSeverity unitySeverity, Severity bugsnagSeverity) {
        customMapping[unitySeverity] = bugsnagSeverity;
    }

    // Ability to set the maximum count for a log type in the rate limit period
    public static void SetMaximumCount(LogType unityLogType, int maxCount) {
        maxCounts[unityLogType] = maxCount;
    }

    // Used to set the API key when Bugsnag is being initialized
    public static void SetApiKey(String apiKey) {
        BugsnagApiKeyStatic = apiKey;
    }

    // Used to set the session defaults when Bugsnag is being initialized
    public static void SetAutoCaptureSessions(bool enabled) {
        TrackAppSessionsStatic = enabled;
    }

    string GetLevelName() {
#if UNITY_5_OR_NEWER
      return SceneManager.GetActiveScene().name;
#else
      return Application.loadedLevelName;
#endif
    }

    void Awake() {
        DontDestroyOnLoad(this);
        Init(null);
    }

    public static Bugsnag createBugsnagInstance(GameObject gameObject, String ApiKey) {
        return createBugsnagInstance(gameObject, ApiKey, false);
    }

    public static Bugsnag createBugsnagInstance(GameObject gameObject, String ApiKey, bool autoCaptureSessions) {
        Bugsnag.SetApiKey(ApiKey);
        Bugsnag.SetAutoCaptureSessions(autoCaptureSessions);
        Bugsnag bugsnagInstance = gameObject.AddComponent<Bugsnag>();
        bugsnagInstance.Init();
        return bugsnagInstance;
    }

    public void Init() {
        Init(null);
    }

    public void Init(string apiKey) {
        // Try to work out which API key to use
        if (!String.IsNullOrEmpty (apiKey)) {
            InitInternal (apiKey);
        } else if (!String.IsNullOrEmpty(BugsnagApiKey)) {
            InitInternal(BugsnagApiKey);
        } else if (!String.IsNullOrEmpty(BugsnagApiKeyStatic)) {
            InitInternal(BugsnagApiKeyStatic);
        } else {
            Debug.LogError("BUGSNAG: ERROR: unable to initialize Bugsnag, API key must be specified");
        }
    }

    private void InitInternal(string apiKey) {
        BugsnagApiKey = apiKey;
        NativeBugsnag.Register(BugsnagApiKey, TrackAppSessions || TrackAppSessionsStatic);

        if(BugsnagReleaseStageStatic != null) {
            Bugsnag.ReleaseStage = BugsnagReleaseStageStatic;
        } else if(Debug.isDebugBuild) {
            Bugsnag.ReleaseStage = "development";
        } else {
            Bugsnag.ReleaseStage = "production";
        }

#if (UNITY_WEBGL) && !UNITY_EDITOR
            // Android/Cocoa platforms are able to get the app version automatically
            // but WebGL needs to have it explicitly passed through to the JS layer
            NativeBugsnag.SetAppVersion(Application.version);
#endif

        Bugsnag.Context = GetLevelName();
        NativeBugsnag.SetAutoNotify (AutoNotify);
        NativeBugsnag.AddToTab("Unity", "unityException", "false");
        NativeBugsnag.AddToTab("Unity", "unityVersion", Application.unityVersion.ToString());
        NativeBugsnag.AddToTab("Unity", "platform", Application.platform.ToString());
        NativeBugsnag.AddToTab("Unity", "osLanguage", Application.systemLanguage.ToString());
#if UNITY_5_OR_NEWER
#if UNITY_5_6_OR_NEWER
        NativeBugsnag.AddToTab("Unity", "bundleIdentifier", Application.identifier.ToString());
#else
        NativeBugsnag.AddToTab("Unity", "bundleIdentifier", Application.bundleIdentifier.ToString());
#endif
        NativeBugsnag.AddToTab("Unity", "version", Application.version.ToString());
        NativeBugsnag.AddToTab("Unity", "companyName", Application.companyName.ToString());
        NativeBugsnag.AddToTab("Unity", "productName", Application.productName.ToString());
#endif
    }

    void OnEnable () {
#if UNITY_5_4_OR_NEWER
        SceneManager.sceneLoaded += SceneLoaded;
#endif

#if UNITY_5_OR_NEWER
        Application.logMessageReceived += HandleLog;
#else
        Application.RegisterLogCallback(HandleLog);
#endif
    }

    void OnDisable () {
        // Remove callback when object goes out of scope
#if UNITY_5_OR_NEWER
        Application.logMessageReceived -= HandleLog;
#else
        Application.RegisterLogCallback(null);
#endif
    }


#if UNITY_5_4_OR_NEWER
    void SceneLoaded(Scene scene, LoadSceneMode mode) {
        Bugsnag.Context = scene.name;
        LeaveBreadcrumb("Scene loaded: " + scene.name);
    }
#else
    void OnLevelWasLoaded(int level) {
        Bugsnag.Context = GetLevelName();
        LeaveBreadcrumb("Scene loaded: " + GetLevelName());
    }
#endif

    void Update () {
      // Don't bother if limiting unity logs is turned off
      if (!LimitUnityLogs) {
        return;
      }

      // Check if we need to reset the current counts
      if (DateTime.UtcNow - timeCountsLastReset > RateLimitTimePeriod) {
        foreach (LogType key in Enum.GetValues(typeof(LogType))) {
          currentCounts[key] = 0;
        }
        timeCountsLastReset = DateTime.UtcNow;
      }

      // Check if we need to reset the unique logs list
      if (DateTime.UtcNow - timeUniqueLogsLastReset > UniqueLogsTimePeriod) {
          latestLogs = new List<string>();
          timeUniqueLogsLastReset = DateTime.UtcNow;
      }
    }

    void HandleLog (string logString, string stackTrace, LogType type) {
        // Check if we are limiting logs
        if (LimitUnityLogs) {
            // Check if we have exceeded the counts for this type
            if (currentCounts[type] >= maxCounts[type]) {
              return;
            }

            // Check if we have have previously sent this log
            if (latestLogs.Contains(String.Format("{0}|{1}|{2}", logString, stackTrace, type.ToString()))) {
              return;
            }

            // Check if we have sent more than the maximum number of unique logs, and if so throw them away
            if (latestLogs.Count >= maxUniqueLogCount) {
              return;
            }

            currentCounts[type] += 1;
            latestLogs.Add(String.Format("{0}|{1}|{2}", logString, stackTrace, type.ToString()));
        }

        // Use any custom log mapping, and if there isn't one, use the default mapping
        LogSeverity logSeverity = logTypeMapping[type];
        Severity bugsnagSeverity = Severity.Error;
        if (customMapping.ContainsKey(logSeverity)) {
            bugsnagSeverity = customMapping[logSeverity];
        } else if (defaultMapping.ContainsKey(logSeverity)) {
            bugsnagSeverity = defaultMapping[logSeverity];
        }

        if(logSeverity >= NotifyLevel) {
            string errorClass, errorMessage = "";

            Regex exceptionRegEx = new Regex(@"^(?<errorClass>\S+):\s*(?<message>.*)", RegexOptions.Singleline);
            Match match = exceptionRegEx.Match(logString);

            if(match.Success) {
                errorClass = match.Groups["errorClass"].Value;
                errorMessage = match.Groups["message"].Value.Trim();
            } else {
                errorClass = "UnityLog" + type;
                errorMessage = logString;
            }

            if (stackTrace == null || stackTrace == "") {
                stackTrace = new System.Diagnostics.StackTrace (1, true).ToString ();
            }

            NotifySafely (errorClass, errorMessage, bugsnagSeverity, "", stackTrace, type, "log");
        }
    }

    public static void Notify(Exception e) {
        if(e != null) {
            var stackTrace = e.StackTrace;
            if (stackTrace == null) {
                stackTrace = new System.Diagnostics.StackTrace (1, true).ToString ();
            }

            NotifySafely (e.GetType ().ToString (),e.Message, Severity.Warning, "", stackTrace, null, "handledException");
        }
    }

    public static void Notify(Exception e, string context) {
        if(e != null) {
            var stackTrace = e.StackTrace;
            if (stackTrace == null) {
                stackTrace = new System.Diagnostics.StackTrace (1, true).ToString ();
            }

            NotifySafely (e.GetType ().ToString (),e.Message, Severity.Warning, context, stackTrace, null, "handledException");
        }
    }

    public static void AddToTab(string tabName, string attributeName, string attributeValue) {
        if (tabName == null || attributeName == null) {
            return;
        }
        if (attributeValue == null) {
            attributeValue = "null";
        }
        NativeBugsnag.AddToTab(tabName, attributeName, attributeValue);
    }

    public static void ClearTab(string tabName) {
        if (tabName == null) {
            return;
        }
        NativeBugsnag.ClearTab(tabName);
    }

    public static void LeaveBreadcrumb(string breadcrumb) {
        if (breadcrumb == null) {
            return;
        }
        NativeBugsnag.LeaveBreadcrumb(breadcrumb);
    }

    public static int BreadcrumbCapacity {
        set { NativeBugsnag.SetBreadcrumbCapacity(value); }
    }

    public static string AppVersion {
        set {
          if (value == null) {
              value = "";
          }
          NativeBugsnag.SetAppVersion(value);
        }
    }

    public static void SetUser(string userId, string userName, string userEmail) {
        if (userId == null) {
            userId = "[unknown]";
        }

        if (userName == null) {
            userName = "[unknown]";
        }

        if (userEmail == null) {
            userEmail = "[unknown]";
        }

        NativeBugsnag.SetUser(userId, userName, userEmail);
    }

    private static void NotifySafely(string errorClass, string message, Severity severity, string context, string stackTrace, LogType? type, string severityReason) {
        if (errorClass == null) {
            errorClass = "Error";
        }

        if (message == null) {
            message = "";
        }

        if (context == null) {
            context = "";
        }

        if (stackTrace == null) {
            return;
        }

        string logType = "";
        if (type != null) {
            logType = type.ToString();
        }

        NativeBugsnag.Notify(errorClass, message, SeverityValues[(int)severity], context, stackTrace, logType, severityReason);
    }
}
