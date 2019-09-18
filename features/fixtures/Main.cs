using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using BugsnagUnity;
using BugsnagUnity.Payload;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Main : MonoBehaviour {
#if UNITY_EDITOR
  public static void CreateScene() {
    var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects, UnityEditor.SceneManagement.NewSceneMode.Single);
    UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
    var obj = new GameObject("Bugsnag");
    var bugsnag = obj.AddComponent<BugsnagBehaviour>();
    bugsnag.BugsnagApiKey = System.Environment.GetEnvironmentVariable("BUGSNAG_APIKEY");
    bugsnag.AutoCaptureSessions = false;
    obj.AddComponent<Main>();
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/MainScene.unity");
    var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
    scenes.Add(new EditorBuildSettingsScene("Assets/MainScene.unity", true));
    EditorBuildSettings.scenes = scenes.ToArray();
  }
#endif

  [DllImport ("NativeCrashy")]
  private static extern void crashy_signal_runner(float num);

  bool sent = false;

  public void Start() {
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
  }

  void Update() {
    // only send one crash
    if (!sent) {
      var scenario = Environment.GetEnvironmentVariable("BUGSNAG_SCENARIO");
      Bugsnag.Configuration.AutoCaptureSessions = scenario.Contains("AutoSession");
      sent = true;
      var endpoint =
        new System.Uri(Environment.GetEnvironmentVariable("MAZE_ENDPOINT"));
      Bugsnag.Configuration.Endpoint = endpoint;
      Bugsnag.Configuration.SessionEndpoint = endpoint;
      LoadScenario();
      // wait for 5 seconds before exiting the application
      StartCoroutine(WaitForBugsnag());
    }
  }

  IEnumerator WaitForBugsnag() {
    yield return new WaitForSeconds(5);
    Application.Quit();
  }

  void LoadScenario() {
    var scenario = Environment.GetEnvironmentVariable("BUGSNAG_SCENARIO");
    switch (scenario) {
      case "LogExceptionOutsideNotifyReleaseStages":
        Bugsnag.Configuration.ReleaseStage = "dev";
        Bugsnag.Configuration.NotifyReleaseStages = new [] { "production" };
        DoLogUnthrown();
        break;
      case "NotifyOutsideNotifyReleaseStages":
        Bugsnag.Configuration.ReleaseStage = "dev";
        Bugsnag.Configuration.NotifyReleaseStages = new [] { "production" };
        DoNotify();
        break;
      case "NativeCrashOutsideNotifyReleaseStages":
        Bugsnag.Configuration.ReleaseStage = "dev";
        Bugsnag.Configuration.NotifyReleaseStages = new [] { "production" };
        crashy_signal_runner(8);
        break;
      case "UncaughtExceptionOutsideNotifyReleaseStages":
        Bugsnag.Configuration.ReleaseStage = "dev";
        Bugsnag.Configuration.NotifyReleaseStages = new [] { "production" };
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
        Bugsnag.Configuration.ReleaseStage = "production";
        Bugsnag.Configuration.NotifyReleaseStages = new [] { "production" };
        break;
      case "ManualSessionInNotifyReleaseStages":
        Bugsnag.Configuration.ReleaseStage = "production";
        Bugsnag.Configuration.NotifyReleaseStages = new [] { "production" };
        Bugsnag.StartSession();
        break;
      case "AutoSessionNotInNotifyReleaseStages":
        Bugsnag.Configuration.NotifyReleaseStages = new [] { "no-op" };
        break;
      case "ManualSessionNotInNotifyReleaseStages":
        Bugsnag.Configuration.NotifyReleaseStages = new [] { "no-op" };
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
        Bugsnag.Configuration.AutoNotify = false;
        DoUnhandledException(0);
        break;
      case "NotifyWithoutAutoNotify":
        Bugsnag.Configuration.AutoNotify = false;
        DoNotify();
        break;
      case "LoggedExceptionWithoutAutoNotify":
        Bugsnag.Configuration.AutoNotify = false;
        DoLogUnthrownAsUnhandled();
        break;
      case "NativeCrashWithoutAutoNotify":
        Bugsnag.Configuration.AutoNotify = false;
        crashy_signal_runner(8);
        break;
      case "NativeCrashReEnableAutoNotify":
        Bugsnag.Configuration.AutoNotify = false;
        Bugsnag.Configuration.AutoNotify = true;
        crashy_signal_runner(8);
        break;
      case "AutoSessionNativeCrash":
        new Thread(() => {
          Thread.Sleep(900);
          crashy_signal_runner(8);
        }).Start();
        break;
    }
  }

  void RunResumedSession() {
    // send 1st exception which should include session info
    Bugsnag.StartSession();
    Bugsnag.Notify(new System.Exception("First Error"));

    // send 2nd exception after resuming a session
    Bugsnag.StopSession();
    Bugsnag.ResumeSession();
    Bugsnag.Notify(new System.Exception("Second Error"));
  }

  void RunNewSession() {
    // send 1st exception which should include session info
    Bugsnag.StartSession();
    Bugsnag.Notify(new System.Exception("First Error"));

    // stop tracking the existing session
    Bugsnag.StopSession();
    Bugsnag.StartSession();

    // send 2nd exception which should contain new session info
    Bugsnag.Notify(new System.Exception("Second Error"));
  }

  void UncaughtExceptionAsUnhandled() {
    Bugsnag.Configuration.ReportUncaughtExceptionsAsHandled = false;
    throw new ExecutionEngineException("Invariant state failure");
  }

  void DoLogWarning() {
    Bugsnag.Configuration.NotifyLevel = LogType.Warning;
    Debug.LogWarning("Something went terribly awry");
  }

  void DoLogError() {
    Bugsnag.Configuration.NotifyLevel = LogType.Warning;
    Debug.LogError("Bad bad things");
  }

  void DoLogWarningWithHandledConfig() {
    Bugsnag.Configuration.ReportUncaughtExceptionsAsHandled = false;
    Bugsnag.Configuration.NotifyLevel = LogType.Warning;
    Debug.LogWarning("Something went terribly awry");
  }

  void LeaveComplexBreadcrumbAndNotify() {
    Bugsnag.LeaveBreadcrumb("Reload", BreadcrumbType.Navigation, new Dictionary<string, string>() {
      { "preload", "launch" }
    });
    Bugsnag.Notify(new System.Exception("Collective failure"));
  }

  void NotifyTwice() {
    StartCoroutine(NotifyTwiceCoroutine());
  }

  IEnumerator NotifyTwiceCoroutine() {
    Bugsnag.Notify(new System.Exception("Rollback failed"));
    yield return new WaitForSeconds(1);
    Bugsnag.Notify(new ExecutionEngineException("Invalid runtime"));
  }

  void LeaveMessageBreadcrumbAndNotify() {
    Bugsnag.LeaveBreadcrumb("Initialize bumpers");
    Bugsnag.Notify(new System.Exception("Collective failure"));
  }

  void LogLowLevelMessageAndNotify() {
    Debug.LogWarning("Failed to validate credentials");
    Bugsnag.Notify(new ExecutionEngineException("Invalid runtime"));
  }

  void DoNotifyWithCallback() {
    Bugsnag.Notify(new System.Exception("blorb"), report => {
      report.Exceptions[0].ErrorClass = "FunnyBusiness";
      report.Exceptions[0].ErrorMessage = "cake";
      report.Metadata.Add("shape", new Dictionary<string, string>() {
        { "arc", "yes" },
      });
    });
  }

  void DoNotifyWithSeverity() {
    Bugsnag.Notify(new System.Exception("blorb"), Severity.Info);
  }

  void DoNotify() {
    Bugsnag.Notify(new System.Exception("blorb"));
  }

  void DoLogUnthrownAsUnhandled() {
    Bugsnag.Configuration.ReportUncaughtExceptionsAsHandled = false;
    Debug.LogException(new System.Exception("WAT"));
  }

  void DoLogUnthrown() {
    Debug.LogException(new System.Exception("auth failed!"));
  }

  void DoUnhandledException(long counter) {
    var items = new int[]{1, 2, 3};
    Debug.Log("Item #1 is: " + items[counter]);
    throw new ExecutionEngineException("Promise Rejection");
  }

  void MakeAssertionFailure(int counter) {
    var items = new int[]{1, 2, 3};
    Debug.Log("Item4 is: " + items[counter]);
  }
}

class OtherMetadata {
  public override string ToString() {
    return "more stuff";
  }
}

