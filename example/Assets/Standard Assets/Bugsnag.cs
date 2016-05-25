#if UNITY_2_6 || UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
#define UNITY_LT_5
#endif

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using System.Text.RegularExpressions;

public class Bugsnag : MonoBehaviour {

#if UNITY_STANDALONE_OSX
    private const string dllName = "bugsnag-osx";
#else
    private const string dllName = "__Internal";
#endif

    public class NativeBugsnag {
        #if (UNITY_IPHONE || UNITY_IOS || UNITY_WEBGL || UNITY_STANDALONE_OSX) && !UNITY_EDITOR
            [DllImport (dllName)]
            public static extern void Register(string apiKey);

            [DllImport (dllName)]
            public static extern void Notify(string errorClass, string errorMessage, string severity, string context, string stackTrace);

            [DllImport (dllName)]
            public static extern void SetNotifyUrl(string notifyUrl);

            [DllImport (dllName)]
            public static extern void SetAutoNotify(bool autoNotify);

            [DllImport (dllName)]
            public static extern void SetContext(string context);

            [DllImport (dllName)]
            public static extern void SetReleaseStage(string releaseStage);

            [DllImport (dllName)]
            public static extern void SetNotifyReleaseStages(string releaseStages);

            [DllImport (dllName)]
            public static extern void AddToTab(string tabName, string attributeName, string attributeValue);

            [DllImport (dllName)]
            public static extern void ClearTab(string tabName);

            [DllImport (dllName)]
            public static extern void LeaveBreadcrumb(string breadcrumb);

            [DllImport (dllName)]
            public static extern void SetBreadcrumbCapacity(int capacity);

            [DllImport (dllName)]
            public static extern void SetAppVersion(string appVersion);

            [DllImport (dllName)]
            public static extern void SetUser(string userId, string userName, string userEmail);
        #elif UNITY_ANDROID && !UNITY_EDITOR
            public static AndroidJavaClass Bugsnag = new AndroidJavaClass("com.bugsnag.android.Bugsnag");
            public static Regex unityExpression = new Regex ("(\\S+)\\s*\\(.*?\\)\\s*(?:(?:\\[.*\\]\\s*in\\s|\\(at\\s*\\s*)(.*):(\\d+))?", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            public static void Register(string apiKey) {
                // Get the current Activity
                AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject app = activity.Call<AndroidJavaObject>("getApplicationContext");

                Bugsnag.CallStatic<AndroidJavaObject> ("init", app, apiKey);
                Notify("errorClass", "error message", "error", "", new System.Diagnostics.StackTrace (1, true).ToString (), true);
            }

            public static void Notify(string errorClass, string errorMessage, string severity, string context, string stackTrace) {
              Notify(errorClass, errorMessage, severity, context, stackTrace, false);
            }

            public static void Notify(string errorClass, string errorMessage, string severity, string context, string stackTrace, bool warmup) {
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

                    var metaData = new AndroidJavaObject("com.bugsnag.android.MetaData");

                    // Build the arguments
                    jvalue[] args =  new jvalue[6] {
                        new jvalue() { l = AndroidJNI.NewStringUTF(errorClass) },
                        new jvalue() { l = AndroidJNI.NewStringUTF(errorMessage) },
                        new jvalue() { l = AndroidJNI.NewStringUTF(context) },
                        new jvalue() { l = (IntPtr)stackFrameArrayObject },
                        new jvalue() { l = severityInstance.GetRawObject() },
                        new jvalue() { l = metaData.GetRawObject() }
                    };

                    // Call Android's notify method
                    IntPtr clientConstructorId = AndroidJNI.GetStaticMethodID(Bugsnag.GetRawClass(), "notify", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;[Ljava/lang/StackTraceElement;Lcom/bugsnag/android/Severity;Lcom/bugsnag/android/MetaData;)V");
                    if(warmup == false) AndroidJNI.CallStaticVoidMethod(Bugsnag.GetRawClass(), clientConstructorId, args);
                }
            }

            public static void SetNotifyUrl(string notifyUrl) {
                Bugsnag.CallStatic ("setEndpoint", notifyUrl);
            }

            public static void SetAutoNotify(bool autoNotify) {
                if (autoNotify) {
                    Bugsnag.CallStatic ("enableExceptionHandler");
                } else {
                    Bugsnag.CallStatic ("disableExceptionHandler");
                }
            }

            public static void SetContext(string context) {
                Bugsnag.CallStatic ("setContext", context);
            }

            public static void SetReleaseStage(string releaseStage) {
                Bugsnag.CallStatic ("setReleaseStage", releaseStage);
            }

            public static void SetNotifyReleaseStages(string releaseStages) {
                Bugsnag.CallStatic ("setNotifyReleaseStages", releaseStages.Split (','));
            }

            public static void AddToTab(string tabName, string attributeName, string attributeValue) {
                Bugsnag.CallStatic ("addToTab", tabName, attributeName, attributeValue);
            }

            public static void ClearTab(string tabName) {
                Bugsnag.CallStatic ("clearTab", tabName);
            }

            public static void LeaveBreadcrumb(string breadcrumb) {
                Bugsnag.CallStatic ("leaveBreadcrumb", breadcrumb);
            }

            public static void SetBreadcrumbCapacity(int capacity) {
                Bugsnag.CallStatic ("setMaxBreadcrumbs", capacity);
            }

            public static void SetAppVersion(string version) {
                Bugsnag.CallStatic ("setAppVersion", version);
            }

            public static void SetUser(string userId, string userName, string userEmail) {
                Bugsnag.CallStatic ("setUser", userId, userEmail, userName);
            }
        #else
            private static string apiKey_;

            public static void SetContext(string context) {}
            public static void SetReleaseStage(string releaseStage) {}
            public static void SetNotifyReleaseStages(string releaseStages) {}
            public static void Notify(string errorClass, string errorMessage, string severity, string context, string stackTrace) {
                if (apiKey_ == null || apiKey_ == "") {
                    Debug.Log("BUGSNAG: ERROR: would not notify Bugsnag as no API key was set");
                } else {
                    Debug.Log("BUGSNAG: Would notify Bugsnag about " + errorClass + ": " + errorMessage);
                }
            }
            public static void Register(string apiKey) {
                apiKey_ = apiKey;
            }
            public static void SetNotifyUrl(string notifyUrl) {}
            public static void SetAutoNotify(bool autoNotify) {}
            public static void AddToTab(string tabName, string attributeName, string attributeValue) {}
            public static void ClearTab(string tabName) {}
            public static void LeaveBreadcrumb(string breadcrumb) {}
            public static void SetBreadcrumbCapacity(int capacity) {}
            public static void SetAppVersion(string version) {}
            public static void SetUser(string userId, string userName, string userEmail) {}
        #endif
    }

    // We dont use the LogType enum in Unity as the numerical order doesnt suit our purposes
    public enum LogSeverity {
        Log,
        Warning,
        Assert,
        Error,
        Exception
    }

    public string BugsnagApiKey = "";
    public bool AutoNotify = true;
    public LogSeverity NotifyLevel = LogSeverity.Exception;

    public static string ReleaseStage {
        set {
            if (value == null) {
                value = "production";
            }
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

    public static string[] NotifyReleaseStages {
        set {
            NativeBugsnag.SetNotifyReleaseStages(String.Join (",", value));
        }
    }

    string GetLevelName() {
#if UNITY_LT_5
      return Application.loadedLevelName;
#else
      return SceneManager.GetActiveScene().name;
#endif
    }

    void Awake() {
        DontDestroyOnLoad(this);
        NativeBugsnag.Register(BugsnagApiKey);

        if(Debug.isDebugBuild) {
            Bugsnag.ReleaseStage = "development";
        } else {
            Bugsnag.ReleaseStage = "production";
        }

        Bugsnag.Context = GetLevelName();
        NativeBugsnag.SetAutoNotify (AutoNotify);
        NativeBugsnag.AddToTab("Unity", "unityVersion", Application.unityVersion.ToString());
        NativeBugsnag.AddToTab("Unity", "bundleIdentifier", Application.bundleIdentifier.ToString());
        NativeBugsnag.AddToTab("Unity", "version", Application.version.ToString());
        NativeBugsnag.AddToTab("Unity", "companyName", Application.companyName.ToString());
        NativeBugsnag.AddToTab("Unity", "platform", Application.platform.ToString());
        NativeBugsnag.AddToTab("Unity", "productName", Application.productName.ToString());
        NativeBugsnag.AddToTab("Unity", "osLanguage", Application.systemLanguage.ToString());
    }

    void OnEnable () {
#if UNITY_LT_5
        Application.RegisterLogCallback(HandleLog);
#else
        Application.logMessageReceived += HandleLog;
#endif
    }

    void OnDisable () {
        // Remove callback when object goes out of scope
#if UNITY_LT_5
        Application.RegisterLogCallback(null);
#else
        Application.logMessageReceived -= HandleLog;
#endif
    }

    void OnLevelWasLoaded(int level) {
        Bugsnag.Context = GetLevelName();
    }

    void HandleLog (string logString, string stackTrace, LogType type) {
        LogSeverity severity = LogSeverity.Exception;
        var bugsnagSeverity = "error";

        switch (type) {
        case LogType.Assert:
            severity = LogSeverity.Assert;
            break;
        case LogType.Error:
            severity = LogSeverity.Error;
            break;
        case LogType.Exception:
            severity = LogSeverity.Exception;
            break;
        case LogType.Log:
            severity = LogSeverity.Log;
            bugsnagSeverity = "info";

            break;
        case LogType.Warning:
            severity = LogSeverity.Warning;
            bugsnagSeverity = "warning";
            break;
        default:
            break;
        }

        if(severity >= NotifyLevel && AutoNotify) {
            string errorClass, errorMessage = "";

            Regex exceptionRegEx = new Regex(@"^(?<errorClass>\S+):\s*(?<message>.*)");
            Match match = exceptionRegEx.Match(logString);

            if(match.Success) {
                errorClass = match.Groups["errorClass"].Value;
                errorMessage = match.Groups["message"].Value.Trim();
            } else {
                errorClass = logString;
            }

            if (stackTrace == null || stackTrace == "") {
                stackTrace = new System.Diagnostics.StackTrace (1, true).ToString ();
            }

            NotifySafely (errorClass, errorMessage, bugsnagSeverity, "", stackTrace);
        }
    }

    public static void Notify(Exception e) {
        if(e != null) {
            var stackTrace = e.StackTrace;
            if (stackTrace == null) {
                stackTrace = new System.Diagnostics.StackTrace (1, true).ToString ();
            }

            NotifySafely (e.GetType ().ToString (),e.Message, "warning", "", stackTrace);
        }
    }

    public static void Notify(Exception e, string context) {
        if(e != null) {
            var stackTrace = e.StackTrace;
            if (stackTrace == null) {
                stackTrace = new System.Diagnostics.StackTrace (1, true).ToString ();
            }

            NotifySafely (e.GetType ().ToString (),e.Message, "warning", context, stackTrace);
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

    private static void NotifySafely(string errorClass, string message, string severity, string context, string stackTrace) {
        if (errorClass == null) {
            errorClass = "Error";
        }

        if (message == null) {
            message = "";
        }

        if (severity == null) {
            severity = "error";
        }

        if (context == null) {
            context = "";
        }

        if (stackTrace == null) {
            return;
        }

        NativeBugsnag.Notify(errorClass, message, severity, context, stackTrace);
    }
}
