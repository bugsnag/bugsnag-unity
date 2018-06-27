using System;
using UnityEngine;

namespace Bugsnag.Unity
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

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// We use this to pull the fields that have been set in the
    /// Unity editor and pass them to the Bugsnag client.
    /// </summary>
    void Awake()
    {
      Bugsnag.Init(BugsnagApiKey);
      Bugsnag.Client.Configuration.AutoNotify = AutoNotify;
      Bugsnag.Client.Configuration.UniqueLogsTimePeriod = TimeSpan.FromSeconds(UniqueLogsPerSecond);
      Bugsnag.Client.Configuration.NotifyLevel = NotifyLevel;
      Bugsnag.Client.Configuration.ReleaseStage = Debug.isDebugBuild ? "debug" : "production";
      Bugsnag.Client.Configuration.MaximumBreadcrumbs = MaximumBreadcrumbs;
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
      if (hasFocus)
      {
        Bugsnag.Client.SessionTracking.StartSession();
      }
    }
  }


#if UNITY_EDITOR
  [UnityEditor.CustomEditor(typeof(Bugsnag))]
  [UnityEditor.CanEditMultipleObjects]
  public class BugsnagEditor : UnityEditor.Editor
  {
    UnityEditor.SerializedProperty apiKey;
    UnityEditor.SerializedProperty autoNotify;
    UnityEditor.SerializedProperty notifyLevel;
    UnityEditor.SerializedProperty maximumBreadcrumbs;
    UnityEditor.SerializedProperty uniqueLogsPerSecond;

    void OnEnable()
    {
      apiKey = serializedObject.FindProperty("BugsnagApiKey");
      autoNotify = serializedObject.FindProperty("AutoNotify");
      notifyLevel = serializedObject.FindProperty("NotifyLevel");
      maximumBreadcrumbs = serializedObject.FindProperty("MaximumBreadcrumbs");
      uniqueLogsPerSecond = serializedObject.FindProperty("UniqueLogsPerSecond");
    }

    public override void OnInspectorGUI()
    {
      serializedObject.Update();

      UnityEditor.EditorGUILayout.PropertyField(apiKey, new GUIContent("API Key"));
      UnityEditor.EditorGUILayout.PropertyField(autoNotify, new GUIContent("Auto Notify"));
      UnityEditor.EditorGUILayout.PropertyField(notifyLevel, new GUIContent("Notify Level"));
      UnityEditor.EditorGUILayout.PropertyField(maximumBreadcrumbs, new GUIContent("Maximum Breadcrumbs"));
      UnityEditor.EditorGUILayout.PropertyField(uniqueLogsPerSecond, new GUIContent("uniqueLogsPerSecond"));

      serializedObject.ApplyModifiedProperties();
    }
  }
#endif
}
