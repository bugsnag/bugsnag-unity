using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
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
    obj.AddComponent<Main>();
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/MainScene.unity");
    var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
    scenes.Add(new EditorBuildSettingsScene("Assets/MainScene.unity", true));
    EditorBuildSettings.scenes = scenes.ToArray();
  }
#endif

  bool sent = false;

  void Update() {
    // only send one crash
    if (!sent) {
      sent = true;
      Bugsnag.Configuration.Endpoint =
        new System.Uri(Environment.GetEnvironmentVariable("MAZE_ENDPOINT"));
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
      case "LogUnthrown":
        DoLogUnthrown();
        break;
      case "UncaughtException":
        DoUnhandledException(0);
        break;
      case "AssertionFailure":
        MakeAssertionFailure(4);
        break;
    }
  }

  void LeaveComplexBreadcrumbAndNotify() {
    Bugsnag.LeaveBreadcrumb("Reload", BreadcrumbType.Navigation, new Dictionary<string, string>() {
      { "preload", "launch" }
    });
    Bugsnag.Notify(new System.Exception("Collective failure"));
  }

  void NotifyTwice() {
    Bugsnag.Notify(new System.Exception("Rollback failed"));
    System.Threading.Thread.Sleep(20);
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

  void DoNotify() {
    Bugsnag.Notify(new System.Exception("blorb"));
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

