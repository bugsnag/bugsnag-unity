using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif

namespace BugsnagUnity
{
  public class BugsnagBehaviour : MonoBehaviour
  {
    /// <summary>
    /// Exposed in the Unity Editor to configure this behaviour
    /// </summary>
    public string BugsnagApiKey = "";

    /// <summary>
    /// Exposed in the Unity Editor to configure this behaviour
    /// </summary>
    public bool AutoNotify = true;

    public LogType NotifyLevel = LogType.Exception;

    public int MaximumBreadcrumbs = 25;

    public int UniqueLogsPerSecond = 5;

    public bool AutoCaptureSessions = true;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// We use this to pull the fields that have been set in the
    /// Unity editor and pass them to the Bugsnag client.
    /// </summary>
    void Awake()
    {
      Bugsnag.Init(BugsnagApiKey);
      Bugsnag.Configuration.AutoNotify = AutoNotify;
      Bugsnag.Configuration.AutoCaptureSessions = AutoCaptureSessions;
      Bugsnag.Configuration.UniqueLogsTimePeriod = TimeSpan.FromSeconds(UniqueLogsPerSecond);
      Bugsnag.Configuration.NotifyLevel = NotifyLevel;
      Bugsnag.Configuration.ReleaseStage = Debug.isDebugBuild ? "development" : "production";
      Bugsnag.Configuration.MaximumBreadcrumbs = MaximumBreadcrumbs;

      Bugsnag.Configuration.ScriptingBackend = FindScriptingBackend();
      Bugsnag.Configuration.DotnetScriptingRuntime = FindDotnetScriptingRuntime();
      Bugsnag.Configuration.DotnetApiCompatibility = FindDotnetApiCompatibility();
    }

    /// <summary>
    /// OnApplicationFocus is called when the application loses or gains focus.
    /// Alt-tabbing or Cmd-tabbing can take focus away from the Unity
    /// application to another desktop application. This causes the GameObjects
    /// to receive an OnApplicationFocus call with the argument set to false.
    /// When the user switches back to the Unity application, the GameObjects
    /// receive an OnApplicationFocus call with the argument set to true.
    /// </summary>
    /// <param name="hasFocus"></param>
    void OnApplicationFocus(bool hasFocus)
    {
      Bugsnag.SetApplicationState(hasFocus);
    }

    void OnApplicationPause(bool paused)
    {
      var hasFocus = !paused;
      Bugsnag.SetApplicationState(hasFocus);
    }

    /*** Determine runtime versions ***/

    private static string FindScriptingBackend() {
#if ENABLE_MONO
      return "Mono";
#elif ENABLE_IL2CPP
      return "IL2CPP";
#else
      return "Unknown";
#endif
    }

    private static string FindDotnetScriptingRuntime() {
#if NET_4_6
      return ".NET 4.6 equivalent";
#else
      return ".NET 3.5 equivalent";
#endif
    }

    private static string FindDotnetApiCompatibility() {
#if NET_2_0_SUBSET
      return ".NET 2.0 Subset";
#else
      return ".NET 2.0";
#endif
    }
  }

#if UNITY_EDITOR
  [CustomEditor(typeof(BugsnagBehaviour))]
  [CanEditMultipleObjects]
  public class BugsnagBehaviourEditor : Editor
  {
    bool showAdvanced;
    SerializedProperty apiKey;
    SerializedProperty notifyLevel;
    SerializedProperty maximumBreadcrumbs;
    SerializedProperty autoNotify;
    SerializedProperty autoCaptureSessions;
    SerializedProperty uniqueLogsPerSecond;

    void OnEnable()
    {
      showAdvanced = false;
      apiKey = serializedObject.FindProperty("BugsnagApiKey");
      autoNotify = serializedObject.FindProperty("AutoNotify");
      autoCaptureSessions = serializedObject.FindProperty("AutoCaptureSessions");
      notifyLevel = serializedObject.FindProperty("NotifyLevel");
      maximumBreadcrumbs = serializedObject.FindProperty("MaximumBreadcrumbs");
      uniqueLogsPerSecond = serializedObject.FindProperty("UniqueLogsPerSecond");
    }

    public override void OnInspectorGUI()
    {
      serializedObject.Update();

      EditorGUILayout.PropertyField(apiKey, new GUIContent("API Key"));
      EditorGUILayout.PropertyField(autoNotify, new GUIContent("Auto Notify"));
      EditorGUILayout.PropertyField(autoCaptureSessions, new GUIContent("Auto Capture Sessions"));
      EditorGUILayout.PropertyField(notifyLevel, new GUIContent("Notify Level"));

      showAdvanced = EditorGUILayout.ToggleLeft("Advanced Configuration", showAdvanced);
      if (showAdvanced)
      {
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(maximumBreadcrumbs, new GUIContent("Maximum Breadcrumbs"));
        EditorGUILayout.PropertyField(uniqueLogsPerSecond, new GUIContent("Unique Logs per second", "The number of unique Unity logs per second that Bugsnag will convert to breadcrumbs or report as errors (if configured). Lower the value to address performance problems."));
        EditorGUI.indentLevel--;
      }

      serializedObject.ApplyModifiedProperties();
    }

#if UNITY_IOS || UNITY_TVOS
    [PostProcessBuild(1400)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
      var xcodeProjectPath = Path.Combine(path, "Unity-iPhone.xcodeproj");
      var pbxPath = Path.Combine(xcodeProjectPath, "project.pbxproj");
      var lines = new LinkedList<string>(File.ReadAllLines(pbxPath));
      BugsnagUnity.PostProcessBuild.Apply(lines);
      File.WriteAllLines(pbxPath, lines.ToArray());
    }
#endif
  }
#endif
}
