using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using BugsnagUnity;
using BugsnagUnity.Payload;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Main : MonoBehaviour
{


#if UNITY_STANDALONE_OSX

    [DllImport("NativeCrashy")]
    private static extern void crashy_signal_runner(float num);

#endif

    private Dictionary<string, string> _webGlArguments;


    private string _fakeTrace = "Main.CUSTOM () (at Assets/Scripts/Main.cs:123)\nMain.CUSTOM () (at Assets/Scripts/Main.cs:123)";

    private float _closeTime = 5;

    public void Start()
    {

#if UNITY_ANDROID || UNITY_IOS
        return;
#endif

#if UNITY_WEBGL
        ParseUrlParameters(); 
#endif
        var scenario = GetEvnVar("BUGSNAG_SCENARIO");
        var config = PrepareConfig(scenario);
        Invoke("CloseApplication", _closeTime);
        if (scenario != "ClearBugsnagCache")
        {
            Bugsnag.Start(config);
            Bugsnag.AddMetadata("init", new Dictionary<string, object>(){
                {"foo", "bar" },
            });
            Bugsnag.AddMetadata("test", "test1", "test1");
            Bugsnag.AddMetadata("test", "test2", "test2");
            Bugsnag.AddMetadata("custom", new Dictionary<string, object>(){
                {"letter", "QX" },
                {"better", 400 },
                {"string-array", new string []{"1","2","3"} },
                {"int-array", new int []{1,2,3} },
                {"dict", new Dictionary<string,object>(){ {"test" , 123 } } }
            });
            Bugsnag.AddMetadata("app", new Dictionary<string, object>(){
                {"buildno", "0.1" },
                {"cache", null },
            });
            Bugsnag.ClearMetadata("init");
            Bugsnag.ClearMetadata("test", "test2");
        }
        RunScenario(scenario);
    }

    void CloseApplication()
    {
        Application.Quit();
    }

    private void ParseUrlParameters()
    {

        //Expected url format for webgl tests
        //http://localhost:8888/index.html?BUGSNAG_SCENARIO=MaxBreadcrumbs&BUGSNAG_APIKEY=123344343289478347238947&MAZE_ENDPOINT=http://localhost:9339

        string parameters = Application.absoluteURL.Substring(Application.absoluteURL.IndexOf("?")+1);

        var splitParams = parameters.Split(new char[] { '&', '=' });

        _webGlArguments = new Dictionary<string, string>();

        var currentindex = 0;

        while (currentindex < splitParams.Length)
        {
            _webGlArguments.Add(splitParams[currentindex],splitParams[currentindex + 1]);
            currentindex += 2;
        }
    }

    private string GetWebGLEnvVar(string key)
    {
        foreach (var pair in _webGlArguments)
        {
            if (pair.Key == key)
            {
                return pair.Value;
            }
        }
        throw new System.Exception("COULD NOT GET ENV VAR: " + key);
    }

    Configuration PrepareConfig(string scenario)
    {
        string apiKey = GetEvnVar("BUGSNAG_APIKEY");
        var config = new Configuration(apiKey);

        var endpoint = GetEvnVar("MAZE_ENDPOINT");
        config.Endpoints = new EndpointConfiguration(endpoint + "/notify", endpoint + "/sessions");
        config.AutoTrackSessions = scenario.Contains("AutoSession");

        config.ScriptingBackend = FindScriptingBackend();
        config.DotnetScriptingRuntime = FindDotnetScriptingRuntime();
        config.DotnetApiCompatibility = FindDotnetApiCompatibility();

        PrepareConfigForScenario(config, scenario);
        return config;
    }

    private string GetEvnVar(string key)
    {
#if UNITY_WEBGL
        return GetWebGLEnvVar(key);
#else
        return System.Environment.GetEnvironmentVariable(key);
#endif
    }

    void PrepareConfigForScenario(Configuration config, string scenario)
    {
        switch (scenario)
        {
            case "PersistEvent":
                config.AutoDetectErrors = true;
                config.Endpoints = new EndpointConfiguration("https://notify.def-not-bugsnag.com", "https://notify.def-not-bugsnag.com");
                config.Context = "First Error";
                break;
            case "PersistEventReport":
                config.AutoDetectErrors = true;
                config.Context = "Second Error";
               
                break;
            case "PersistEventReportCallback":
                config.AutoDetectErrors = true;
                config.Context = "Second Error";
                config.AddOnSendError((@event) => {

                    @event.App.BinaryArch = "Persist BinaryArch";

                    @event.Device.Id = "Persist Id";

                    @event.Errors[0].ErrorClass = "Persist ErrorClass";

                    @event.Errors[0].Stacktrace[0].Method = "Persist Method";

                    foreach (var crumb in @event.Breadcrumbs)
                    {
                        crumb.Message = "Persist Message";
                    }

                    @event.AddMetadata("Persist Section", new Dictionary<string, object> { { "Persist Key", "Persist Value" } });

                    return true;
                });
                break;
            case "PersistSession":
                config.AddOnSession((session)=> {
                    session.App.ReleaseStage = "First Session";
                    return true;
                });
                config.Endpoints = new EndpointConfiguration("https://notify.bugsdnag.com", "https://notify.bugsdnag.com");
                config.AutoTrackSessions = true;
                break;
            case "PersistSessionReport":
                config.AddOnSession((session) => {
                    session.App.ReleaseStage = "Second Session";
                    return true;
                });
                config.AutoTrackSessions = true;
                break;
            case "ClearFeatureFlagsInCallback":
                config.AddOnSendError((@event) => {
                    @event.AddFeatureFlag("testName3", "testVariant3");
                    @event.ClearFeatureFlags();
                    return true;
                });
                break;
            case "FeatureFlagsInCallback":
                config.AddFeatureFlag("testName1", "testVariant1");
                config.AddFeatureFlag("testName2", "testVariant1");
                config.AddFeatureFlag("testName2", "testVariant2");
                config.ClearFeatureFlag("testName1");
                config.AddOnSendError((@event)=> {
                    @event.AddFeatureFlag("testName3", "testVariant3");
                    return true;
                });
                break;
            case "FeatureFlagsConfigClearAll":
                config.AddFeatureFlag("testName1", "testVariant1");
                config.AddFeatureFlag("testName2", "testVariant1");
                config.AddFeatureFlag("testName2", "testVariant2");
                config.ClearFeatureFlags();
                break;
            case "FeatureFlagsInConfig":
                config.AddFeatureFlag("testName1", "testVariant1");
                config.AddFeatureFlag("testName2", "testVariant1");
                config.AddFeatureFlag("testName2", "testVariant2");
                config.ClearFeatureFlag("testName1");
                break;
            case "DisableErrorBreadcrumbs":
                config.EnabledBreadcrumbTypes = new BreadcrumbType[] { BreadcrumbType.Log };
                break;
            case "InfLaunchDurationMark":
            case "InfLaunchDuration":
                config.LaunchDurationMillis = 0;
                _closeTime = 8;
                break;
            case "LongLaunchDuration":
                config.LaunchDurationMillis = 10000;
                _closeTime = 12;
                break;
            case "ShortLaunchDuration":
                config.LaunchDurationMillis = 1000;
                _closeTime = 10;
                break;
            case "DisabledReleaseStage":
                config.EnabledReleaseStages = new string[] { "test" };
                config.ReleaseStage = "somevalue";
                break;
            case "EnabledReleaseStage":
                config.EnabledReleaseStages = new string[] { "test" };
                config.ReleaseStage =  "test";
                break;
            case "RedactedKeys":
                config.RedactedKeys = new string[] { "test", "password" };
                break;
            case "CustomAppType":
                config.AppType = "test";
                break;
            case "DiscardErrorClass":
                config.DiscardClasses = new string[] { "ExecutionEngineException" };
                break;
            case "EnableUnhandledExceptions":
                config.EnabledErrorTypes = new EnabledErrorTypes()
                {
                    ANRs = false,
                    AppHangs = false,
                    OOMs = false,
                    Crashes = false,
                    UnityLog = true
                };
                config.NotifyLogLevel = LogType.Exception;
                break;
            case "EnableLogLogs":
                config.EnabledErrorTypes = new EnabledErrorTypes() {
                    ANRs = false,
                    AppHangs = false,
                    OOMs = false,
                    Crashes = false,
                    UnityLog = true
                };
                config.NotifyLogLevel = LogType.Log;
                break;
            case "DisableAllErrorTypes":
                config.AutoDetectErrors = false;
                config.NotifyLogLevel = LogType.Log;
                break;
            case "NewSession":
                config.AutoTrackSessions = false;
                break;
            case "MaxBreadcrumbs":
                config.MaximumBreadcrumbs = 5;
                break;
            case "DisableBreadcrumbs":
                config.EnabledBreadcrumbTypes = new BreadcrumbType[0];
                break;
            case "OnlyLogBreadcrumbs":
                config.EnabledBreadcrumbTypes = new BreadcrumbType[] { BreadcrumbType.Log };
                config.BreadcrumbLogLevel = LogType.Log;
                break;
            case "LogExceptionOutsideNotifyReleaseStages":
                config.ReleaseStage = "dev";
                config.EnabledReleaseStages = new[] { "production" };
                break;
            case "NotifyOutsideNotifyReleaseStages":
                config.ReleaseStage = "dev";
                config.EnabledReleaseStages = new[] { "production" };
                break;
            case "NativeCrashOutsideNotifyReleaseStages":
                config.ReleaseStage = "dev";
                config.EnabledReleaseStages = new[] { "production" };
                break;
            case "UncaughtExceptionOutsideNotifyReleaseStages":
                config.ReleaseStage = "dev";
                config.EnabledReleaseStages = new[] { "production" };
                break;
            case "UncaughtExceptionAsUnhandled":
                config.ReportExceptionLogsAsHandled = false;
                break;
            case "SetUserInConfigNativeCrash":
            case "SetUserInConfigCsharpError":
                config.SetUser("1","2","3");
                break;
            case "LogUnthrownAsUnhandled":
                config.ReportExceptionLogsAsHandled = false;
                break;
            case "ReportLoggedWarning":
                config.NotifyLogLevel = LogType.Warning;
                config.EnabledErrorTypes.UnityLog = true;
                break;
            case "ReportLoggedError":
                config.NotifyLogLevel = LogType.Warning;
                config.EnabledErrorTypes.UnityLog = true;
                break;
            case "ReportLoggedWarningWithHandledConfig":
                config.EnabledErrorTypes.UnityLog = true;
                config.ReportExceptionLogsAsHandled = false;
                config.NotifyLogLevel = LogType.Warning;
                break;
            case "ManualSessionCrash":
                config.ReportExceptionLogsAsHandled = false;
                break;
            case "AutoSessionInNotifyReleaseStages":
                config.ReleaseStage = "production";
                config.EnabledReleaseStages = new[] { "production" };
                break;
            case "ManualSessionInNotifyReleaseStages":
                config.ReleaseStage = "production";
                config.EnabledReleaseStages = new[] { "production" };
                break;
            case "AutoSessionNotInNotifyReleaseStages":
                config.EnabledReleaseStages = new[] { "no-op" };
                break;
            case "ManualSessionNotInNotifyReleaseStages":
                config.EnabledReleaseStages = new[] { "no-op" };
                break;
            case "ManualSessionMixedEvents":
                config.ReportExceptionLogsAsHandled = false;
                config.NotifyLogLevel = LogType.Warning;
                config.EnabledErrorTypes.UnityLog = true;
                break;
            case "UncaughtExceptionWithoutAutoNotify":
                config.AutoDetectErrors = false;
                break;
            case "NotifyWithoutAutoNotify":
                config.AutoDetectErrors = false;
                break;
            case "LoggedExceptionWithoutAutoNotify":
                config.AutoDetectErrors = false;
                config.ReportExceptionLogsAsHandled = false;
                break;
            case "NativeCrashWithoutAutoNotify":
                config.AutoDetectErrors = false;
                break;
            case "NativeCrashReEnableAutoNotify":
                config.AutoDetectErrors = false;
                config.AutoDetectErrors = true;
                break;
            case "ReportLoggedWarningThreaded":
                config.NotifyLogLevel = LogType.Warning;
                config.EnabledErrorTypes.UnityLog = true;
                break;
            case "EventCallbacks":
                config.AddOnError((@event)=> {

                    @event.App.BinaryArch = "BinaryArch";
                    @event.App.BundleVersion = "BundleVersion";
                    @event.App.CodeBundleId = "CodeBundleId";
                    @event.App.DsymUuid = "DsymUuid";
                    @event.App.Id = "Id";
                    @event.App.ReleaseStage = "ReleaseStage";
                    @event.App.Type = "Type";
                    @event.App.Version = "Version";
                    @event.App.InForeground = false;
                    @event.App.IsLaunching = false;

                    @event.Device.Id = "Id";
                    @event.Device.Jailbroken = true;
                    @event.Device.Locale = "Locale";
                    @event.Device.Manufacturer = "Manufacturer";
                    @event.Device.Model = "Model";
                    @event.Device.OsName = "OsName";
                    @event.Device.OsVersion = "OsVersion";
                    @event.Device.FreeDisk = 123;
                    @event.Device.FreeMemory = 123;
                    @event.Device.Orientation = "Orientation";

                    @event.Errors[0].ErrorClass = "ErrorClass";

                    @event.Errors[0].Stacktrace[0].Method = "Method";

                    foreach (var crumb in @event.Breadcrumbs)
                    {
                        crumb.Message = "Custom Message";
                        crumb.Type = BreadcrumbType.Request;
                        crumb.Metadata = new Dictionary<string, object> { {"test", "test" } };
                    }

                    @event.AddMetadata("test1", new Dictionary<string, object> { { "test", "test" } });
                    @event.AddMetadata("test2", new Dictionary<string, object> { { "test", "test" } });
                    @event.ClearMetadata("test2");


                    return true;
                });
                break;
            default: // no special config required
                break;
        }
    }

    /**
     * Runs the crashy code for a given scenario.
     */
    void RunScenario(string scenario)
    {
        switch (scenario)
        {
            case "PersistDeviceId":
                throw new Exception("PersistDeviceId");
            case "FeatureFlagsAfterInitClearAll":
                Bugsnag.AddFeatureFlag("testName1", "testVariant1");
                Bugsnag.AddFeatureFlag("testName2", "testVariant1");
                Bugsnag.AddFeatureFlag("testName2", "testVariant2");
                Bugsnag.ClearFeatureFlags();
                throw new Exception("FeatureFlags");
            case "FeatureFlagsInCallback":
            case "FeatureFlagsConfigClearAll":
            case "FeatureFlagsInConfig":
            case "ClearFeatureFlagsInCallback":
                throw new Exception("FeatureFlags");
            case "FeatureFlagsAfterInit":
                Bugsnag.AddFeatureFlag("testName1", "testVariant1");
                Bugsnag.AddFeatureFlag("testName2", "testVariant1");
                Bugsnag.AddFeatureFlag("testName2", "testVariant2");
                Bugsnag.ClearFeatureFlag("testName1");
                throw new Exception("FeatureFlags");
            case "SetUserAfterInitCsharpError":
                Bugsnag.SetUser("1","2","3");
                Bugsnag.Notify(new Exception("SetUserAfterInitCsharpError"));
                break;
            case "SetUserAfterInitNativeError":
                Bugsnag.SetUser("1", "2", "3");
                MacOSNativeCrash();
                break;
            case "LongLaunchDuration":
            case "ShortLaunchDuration":
                Invoke("LaunchException", 6);
                break;
            case "InfLaunchDurationMark":
                Bugsnag.MarkLaunchCompleted();
                throw new Exception("InfLaunchDurationMark");
                break;
            case "InfLaunchDuration":
                Invoke("LaunchException",6);
                break;
            case "EventCallbacks":
                DoNotify();
                break;
            case "BackgroundThreadCrash":
                BackgroundThreadCrash();
                break;
            case "NotifyWithStrings":
                NotifyWithStrings();
                break;
            case "CustomStacktrace":
                CustomStacktrace();
                break;
            case "DisabledReleaseStage":
            case "EnabledReleaseStage":
                DoNotify();
                break;
            case "CustomAppType":
                DoNotify();
                break;
            case "DiscardErrorClass":
                DoUnhandledException(0);
                break;
            case "EnableUnhandledExceptions":
                Debug.Log("LogLog");
                Debug.LogException(new Exception("LogException"));
                break;
            case "EnableLogLogs":
                Debug.Log("EnableLogLogs");
                break;
            case "DisableAllErrorTypes":
                CheckDisabledErrorTypes();
                break;
            case "MaxBreadcrumbs":
                LeaveBreadcrumbs();
                DoUnhandledException(0);
                break;
            case "OnlyLogBreadcrumbs":
                Debug.Log("Log");
                DoUnhandledException(0);
                break;
            case "DisableBreadcrumbs":
                DoUnhandledException(0);
                break;
            case "LogExceptionOutsideNotifyReleaseStages":
                DoLogUnthrown();
                break;
            case "NotifyOutsideNotifyReleaseStages":
                DoNotify();
                break;
            case "RedactedKeys":
                AddKeysForRedaction();
                DoNotify();
                break;
            case "NativeCrashOutsideNotifyReleaseStages":
                MacOSNativeCrash();
                break;
            case "UncaughtExceptionOutsideNotifyReleaseStages":
                DoUnhandledException(0);
                break;
            case "DebugLogBreadcrumbNotify":
                LogLowLevelMessageAndNotify();
                break;
            case "ComplexBreadcrumbNotify":
                LeaveComplexBreadcrumbAndNotify();
                break;
            case "DoubleNotify":
                NotifyTwice();
                break;
            case "MessageBreadcrumbNotify":
                LeaveMessageBreadcrumbAndNotify();
                break;
            case "Notify":
                DoNotify();
                break;
            case "NotifyBackground":
                new System.Threading.Thread(() => DoNotify()).Start();
                break;
            case "NotifyCallback":
                DoNotifyWithCallback();
                break;
            case "NotifySeverity":
                DoNotifyWithSeverity();
                break;
            case "LogUnthrown":
                DoLogUnthrown();
                break;
            case "SetUserInConfigCsharpError":
            case "UncaughtException":
                DoUnhandledException(0);
                break;
            case "AssertionFailure":
                MakeAssertionFailure(4);
                break;
            case "UncaughtExceptionAsUnhandled":
                ThrowException();
                break;
            case "LogUnthrownAsUnhandled":
                DebugLogException();
                break;
            case "ReportLoggedWarningThreaded":
                new System.Threading.Thread(() => DoLogWarning()).Start();
                break;
            case "ReportLoggedWarning":
                DoLogWarning();
                break;
            case "ReportLoggedError":
                DoLogError();
                break;
            case "ReportLoggedWarningWithHandledConfig":
                DoLogWarningWithHandledConfig();
                break;
            case "ManualSession":
                Bugsnag.StartSession();
                break;
            case "ManualSessionCrash":
                Bugsnag.StartSession();
                ThrowException();
                break;
            case "ManualSessionNotify":
                Bugsnag.StartSession();
                DoNotify();
                break;
            case "AutoSessionInNotifyReleaseStages":
                break;
            case "ManualSessionInNotifyReleaseStages":
                Bugsnag.StartSession();
                break;
            case "AutoSessionNotInNotifyReleaseStages":
                break;
            case "ManualSessionNotInNotifyReleaseStages":
                Bugsnag.StartSession();
                break;
            case "ManualSessionMixedEvents":
                Bugsnag.StartSession();
                DoNotify();
                DoLogWarning();
                ThrowException();
                break;
            case "StoppedSession":
                Bugsnag.StartSession();
                Bugsnag.PauseSession();
                DoNotify();
                break;
            case "ResumedSession":
                RunResumedSession();
                break;
            case "NewSession":
                RunNewSession();
                break;
            case "SetUserInConfigNativeCrash":
            case "NativeCrash":
                MacOSNativeCrash();
                break;
            case "UncaughtExceptionWithoutAutoNotify":
                DoUnhandledException(0);
                break;
            case "NotifyWithoutAutoNotify":
                DoNotify();
                break;
            case "LoggedExceptionWithoutAutoNotify":
                DebugLogException();
                break;
            case "NativeCrashWithoutAutoNotify":
                MacOSNativeCrash();
                break;
            case "NativeCrashReEnableAutoNotify":
                MacOSNativeCrash();
                break;
            case "CheckForManualContextAfterSceneLoad":
                StartCoroutine(SetManualContextReloadSceneAndNotify());
                break;
            case "AutoSessionNativeCrash":
                new Thread(() =>
                {
                    Thread.Sleep(900);
                    MacOSNativeCrash();
                }).Start();
                break;
            case "AutoSession":
                break;
            case "DisableErrorBreadcrumbs":
                DisableErrorBreadcrumbs();
                break;
            case "PersistEvent":
                throw new Exception("First Event");
            case "PersistEventReport":
            case "PersistEventReportCallback":
                throw new Exception("Second Event");
            case "ClearBugsnagCache":
                ClearBugsnagCache();
                break;
            case "PersistSession":
            case "PersistSessionReport":
            case "(noop)":
                break;
            default:
                throw new ArgumentException("Unable to run unexpected scenario: " + scenario);
                break;
        }
    }

    private void ClearBugsnagCache()
    {
        var path = Application.persistentDataPath + "/Bugsnag";
        if(Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    private void DisableErrorBreadcrumbs()
    {
        Debug.Log("1");
        Bugsnag.Notify(new Exception("1"));
        Debug.Log("2");
        Bugsnag.Notify(new Exception("2"));
    }


    private void BackgroundThreadCrash()
    {
        var bgThread = new Thread(()=> { Debug.LogException(new System.Exception("Background Thread Crash"));})
        {
            IsBackground = true
        };
        bgThread.Start();
    }

   

    private void AddKeysForRedaction()
    {
        Bugsnag.AddMetadata("User", new Dictionary<string, object>() {
                    {"test","test" },
                    { "password","password" }
                });
    }

    private void NotifyWithStrings()
    {
        Bugsnag.Notify("CUSTOM","CUSTOM", _fakeTrace);
    }

    private void CustomStacktrace()
    {
        Bugsnag.Notify(new System.Exception("CUSTOM"), _fakeTrace);
    }

    private void CheckEnabledErrorTypes()
    {
        Debug.Log("LogLog");
        Debug.LogWarning("LogWarning");
        Debug.LogError("LogError");
        throw new System.Exception("Exception");
    }

    private void CheckDisabledErrorTypes()
    {
        Debug.Log("LogLog");
        Debug.LogWarning("LogWarning");
        Debug.LogError("LogError");
        Bugsnag.Notify(new System.Exception("Notify"));
        throw new System.Exception("Exception");
    }

    private void LeaveBreadcrumbs()
    {
        for (int i = 0; i < 6; i++)
        {
            Bugsnag.LeaveBreadcrumb("Crumb " + i);
        }
    }

    void RunResumedSession()
    {
        // send 1st exception which should include session info
        Bugsnag.StartSession();
        Bugsnag.Notify(new System.Exception("First Error"));

        // send 2nd exception after resuming a session
        Bugsnag.PauseSession();
        Bugsnag.ResumeSession();
        Bugsnag.Notify(new System.Exception("Second Error"));
    }

    void RunNewSession()
    {
        StartCoroutine(DoRunNewSession());
    }

    private IEnumerator DoRunNewSession()
    {
        // send 1st exception which should include session info
        Bugsnag.StartSession();
        Bugsnag.Notify(new System.Exception("First Error"));

        // stop tracking the existing session
        Bugsnag.PauseSession();
        yield return new WaitForSeconds(1);
        Bugsnag.StartSession();

        // send 2nd exception which should contain new session info
        Bugsnag.Notify(new System.Exception("Second Error"));
    }

    void ThrowException()
    {
        throw new ExecutionEngineException("Invariant state failure");
    }

    void DoLogWarning()
    {
        Debug.LogWarning("Something went terribly awry");
    }

    void DoLogError()
    {
        Debug.LogError("Bad bad things");
    }

    void DoLogWarningWithHandledConfig()
    {
        Debug.LogWarning("Something went terribly awry");
    }

    void LeaveComplexBreadcrumbAndNotify()
    {
        Bugsnag.LeaveBreadcrumb("Reload",  new Dictionary<string, object>() {
      { "preload", "launch" }
    }, BreadcrumbType.Navigation);
        Bugsnag.Notify(new System.Exception("Collective failure"));
    }

    void NotifyTwice()
    {
        StartCoroutine(NotifyTwiceCoroutine());
    }

    IEnumerator NotifyTwiceCoroutine()
    {
        Bugsnag.Notify(new System.Exception("Rollback failed"));
        yield return new WaitForSeconds(1);
        Bugsnag.Notify(new ExecutionEngineException("Invalid runtime"));
    }

    IEnumerator SetManualContextReloadSceneAndNotify()
    {
        Bugsnag.Context = "Manually-Set";
        SceneManager.LoadScene(1,LoadSceneMode.Additive);
        yield return new WaitForSeconds(0.5f);
        Bugsnag.Notify(new System.Exception("ManualContext"));
    }

    void LeaveMessageBreadcrumbAndNotify()
    {
        Bugsnag.LeaveBreadcrumb("Initialize bumpers");
        Bugsnag.Notify(new System.Exception("Collective failure"));
    }

    void LogLowLevelMessageAndNotify()
    {
        Debug.LogWarning("Failed to validate credentials");
        Bugsnag.Notify(new ExecutionEngineException("Invalid runtime"));
    }

    void DoNotifyWithCallback()
    {
        Bugsnag.Notify(new System.Exception("blorb"), @event =>
        {
            @event.Errors[0].ErrorClass = "FunnyBusiness";
            @event.Errors[0].ErrorMessage = "cake";
            @event.AddMetadata("shape", new Dictionary<string, object>() {
        { "arc", "yes" },
        
      });

            return true;
        });
    }

    void DoNotifyWithSeverity()
    {
        Bugsnag.Notify(new System.Exception("blorb"), Severity.Info);
    }

    void DoNotify()
    {
        Bugsnag.Notify(new System.Exception("blorb"));
    }

    void DebugLogException()
    {
        Debug.LogException(new System.Exception("WAT"));
    }

    void DoLogUnthrown()
    {
        Debug.LogException(new System.Exception("auth failed!"));
    }

    void LaunchException()
    {
        throw new Exception("Launch");
    }

    void DoUnhandledException(long counter)
    {
        var items = new int[] { 1, 2, 3 };
        Debug.Log("Item #1 is: " + items[counter]);
        throw new ExecutionEngineException("Promise Rejection");
    }

    void MakeAssertionFailure(int counter)
    {
        var items = new int[] { 1, 2, 3 };
        Debug.Log("Item4 is: " + items[counter]);
    }


    /*** Determine runtime versions ***/

    private static string FindScriptingBackend()
    {
#if ENABLE_MONO
        return "Mono";
#elif ENABLE_IL2CPP
      return "IL2CPP";
#else
      return "Unknown";
#endif
    }

    private static string FindDotnetScriptingRuntime()
    {
#if NET_4_6
      return ".NET 4.6 equivalent";
#else
        return ".NET 3.5 equivalent";
#endif
    }

    private static string FindDotnetApiCompatibility()
    {
#if NET_2_0_SUBSET
        return ".NET 2.0 Subset";
#else
      return ".NET 2.0";
#endif
    }

    private void MacOSNativeCrash()
    {

#if UNITY_STANDALONE_OSX
         crashy_signal_runner(8);
#endif
    }

}



