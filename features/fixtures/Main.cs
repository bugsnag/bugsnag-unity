using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using BugsnagUnity;

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
      case "Notify":
        DoNotify();
        break;
      case "LogUnthrown":
        DoLogUnthrown();
        break;
    }
  }

  void DoNotify() {
    Bugsnag.Notify(new System.Exception("blorb"));
  }

  void DoLogUnthrown() {
    Debug.LogException(new System.Exception("auth failed!"));
  }
}

