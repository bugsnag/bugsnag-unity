using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using BugsnagUnity;
using BugsnagUnity.Payload;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Main : MonoBehaviour
{
    [DllImport("NativeCrashy")]
    private static extern void crashy_signal_runner(float num);

    bool sent = false;

    public void Start()
    {
        var scenario = Environment.GetEnvironmentVariable("BUGSNAG_SCENARIO");
        var config = PrepareConfig(scenario);
        Bugsnag.Start(config);

        // Add different varieties of custom metadata
        Bugsnag.Metadata.Add("init", new Dictionary<string, string>(){
      {"foo", "bar" },
    });
        Bugsnag.Metadata.Add("custom", new Dictionary<string, object>(){
      {"letter", "QX" },
      {"better", 400 },
      {"setter", new OtherMetadata() },
    });
        Bugsnag.Metadata.Add("app", new Dictionary<string, string>(){
      {"buildno", "0.1" },
      {"cache", null },
    });
        // Remove a tab
        Bugsnag.Metadata.Remove("init");

        // trigger the crash
        RunScenario(scenario);
    }

    void Update()
    {
        // only send one crash
        if (!sent)
        {
            sent = true;
            // wait for 5 seconds before exiting the application
            StartCoroutine(WaitForBugsnag());
        }
    }

    IEnumerator WaitForBugsnag()
    {
        yield return new WaitForSeconds(5);
        Application.Quit();
    }

    /**
     * Creates a configuration object and prepares it for the given scenario
     */
    Configuration PrepareConfig(string scenario)
    {
        string apiKey = System.Environment.GetEnvironmentVariable("BUGSNAG_APIKEY");
        var config = new Configuration(apiKey);

        // setup default endpoints etc
        var endpoint = Environment.GetEnvironmentVariable("MAZE_ENDPOINT");
        config.Endpoint = new System.Uri(endpoint + "/notify");
        config.SessionEndpoint = new System.Uri(endpoint + "/sessions");
        config.AutoCaptureSessions = scenario.Contains("AutoSession");

        // replacement for BugsnagBehaviour as not practical to load script in fixture
        config.ScriptingBackend = FindScriptingBackend();
        config.DotnetScriptingRuntime = FindDotnetScriptingRuntime();
        config.DotnetApiCompatibility = FindDotnetApiCompatibility();

        // prepare scenario-specific config
        PrepareConfigForScenario(config, scenario);
        return config;
    }

    /**
     * Prepares the configuration object for a given scenario
     */
    void PrepareConfigForScenario(Configuration config, string scenario)
    {
        switch (scenario)
        {
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
                config.NotifyReleaseStages = new[] { "production" };
                break;
            case "NotifyOutsideNotifyReleaseStages":
                config.ReleaseStage = "dev";
                config.NotifyReleaseStages = new[] { "production" };
                break;
            case "NativeCrashOutsideNotifyReleaseStages":
                config.ReleaseStage = "dev";
                config.NotifyReleaseStages = new[] { "production" };
                break;
            case "UncaughtExceptionOutsideNotifyReleaseStages":
                config.ReleaseStage = "dev";
                config.NotifyReleaseStages = new[] { "production" };
                break;
            case "UncaughtExceptionAsUnhandled":
                config.ReportUncaughtExceptionsAsHandled = false;
                break;
            case "LogUnthrownAsUnhandled":
                config.ReportUncaughtExceptionsAsHandled = false;
                break;
            case "ReportLoggedWarning":
                config.NotifyLevel = LogType.Warning;
                break;
            case "ReportLoggedError":
                config.NotifyLevel = LogType.Warning;
                break;
            case "ReportLoggedWarningWithHandledConfig":
                config.ReportUncaughtExceptionsAsHandled = false;
                config.NotifyLevel = LogType.Warning;
                break;
            case "ManualSessionCrash":
                config.ReportUncaughtExceptionsAsHandled = false;
                break;
            case "AutoSessionInNotifyReleaseStages":
                config.ReleaseStage = "production";
                config.NotifyReleaseStages = new[] { "production" };
                break;
            case "ManualSessionInNotifyReleaseStages":
                config.ReleaseStage = "production";
                config.NotifyReleaseStages = new[] { "production" };
                break;
            case "AutoSessionNotInNotifyReleaseStages":
                config.NotifyReleaseStages = new[] { "no-op" };
                break;
            case "ManualSessionNotInNotifyReleaseStages":
                config.NotifyReleaseStages = new[] { "no-op" };
                break;
            case "ManualSessionMixedEvents":
                config.ReportUncaughtExceptionsAsHandled = false;
                config.NotifyLevel = LogType.Warning;
                break;
            case "UncaughtExceptionWithoutAutoNotify":
                config.AutoNotify = false;
                break;
            case "NotifyWithoutAutoNotify":
                config.AutoNotify = false;
                break;
            case "LoggedExceptionWithoutAutoNotify":
                config.AutoNotify = false;
                config.ReportUncaughtExceptionsAsHandled = false;
                break;
            case "NativeCrashWithoutAutoNotify":
                config.AutoNotify = false;
                break;
            case "NativeCrashReEnableAutoNotify":
                config.AutoNotify = false;
                config.AutoNotify = true;
                break;
            case "ReportLoggedWarningThreaded":
                config.NotifyLevel = LogType.Warning;
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
            case "NativeCrashOutsideNotifyReleaseStages":
                crashy_signal_runner(8);
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
            case "UncaughtException":
                DoUnhandledException(0);
                break;
            case "AssertionFailure":
                MakeAssertionFailure(4);
                break;
            case "UncaughtExceptionAsUnhandled":
                UncaughtExceptionAsUnhandled();
                break;
            case "LogUnthrownAsUnhandled":
                DoLogUnthrownAsUnhandled();
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
                UncaughtExceptionAsUnhandled();
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
                UncaughtExceptionAsUnhandled();
                break;
            case "StoppedSession":
                Bugsnag.StartSession();
                Bugsnag.StopSession();
                DoNotify();
                break;
            case "ResumedSession":
                RunResumedSession();
                break;
            case "NewSession":
                RunNewSession();
                break;
            case "NativeCrash":
                crashy_signal_runner(8);
                break;
            case "UncaughtExceptionWithoutAutoNotify":
                DoUnhandledException(0);
                break;
            case "NotifyWithoutAutoNotify":
                DoNotify();
                break;
            case "LoggedExceptionWithoutAutoNotify":
                DoLogUnthrownAsUnhandled();
                break;
            case "NativeCrashWithoutAutoNotify":
                crashy_signal_runner(8);
                break;
            case "NativeCrashReEnableAutoNotify":
                crashy_signal_runner(8);
                break;
            case "CheckForManualContextAfterSceneLoad":
                StartCoroutine(SetManualContextReloadSceneAndNotify());
                break;
            case "AutoSessionNativeCrash":
                new Thread(() =>
                {
                    Thread.Sleep(900);
                    crashy_signal_runner(8);
                }).Start();
                break;
            case "AutoSession":
                break;
            case "(noop)":
                break;
            default:
                throw new ArgumentException("Unable to run unexpected scenario: " + scenario);
                break;
        }
    }

    private void LeaveBreadcrumbs()
    {
        for (int i = 0; i < 10; i++)
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
        Bugsnag.StopSession();
        Bugsnag.ResumeSession();
        Bugsnag.Notify(new System.Exception("Second Error"));
    }

    void RunNewSession()
    {
        // send 1st exception which should include session info
        Bugsnag.StartSession();
        Bugsnag.Notify(new System.Exception("First Error"));

        // stop tracking the existing session
        Bugsnag.StopSession();
        Bugsnag.StartSession();

        // send 2nd exception which should contain new session info
        Bugsnag.Notify(new System.Exception("Second Error"));
    }

    void UncaughtExceptionAsUnhandled()
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
        Bugsnag.LeaveBreadcrumb("Reload", BreadcrumbType.Navigation, new Dictionary<string, string>() {
      { "preload", "launch" }
    });
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
        Bugsnag.SetContext("Manually-Set");
        SceneManager.LoadScene(0);
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
        Bugsnag.Notify(new System.Exception("blorb"), report =>
        {
            report.Exceptions[0].ErrorClass = "FunnyBusiness";
            report.Exceptions[0].ErrorMessage = "cake";
            report.Metadata.Add("shape", new Dictionary<string, string>() {
        { "arc", "yes" },
      });
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

    void DoLogUnthrownAsUnhandled()
    {
        Debug.LogException(new System.Exception("WAT"));
    }

    void DoLogUnthrown()
    {
        Debug.LogException(new System.Exception("auth failed!"));
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
}

class OtherMetadata
{
    public override string ToString()
    {
        return "more stuff";
    }
}

