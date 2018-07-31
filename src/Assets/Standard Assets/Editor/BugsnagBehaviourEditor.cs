using UnityEditor;
using UnityEngine;

namespace BugsnagUnity
{
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
  }
}
