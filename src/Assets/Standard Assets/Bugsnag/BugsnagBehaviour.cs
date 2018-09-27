#if UNITY_5_3_OR_NEWER || UNITY_5
#define UNITY_5_OR_NEWER
#endif
using System;
using System.IO;
using System.Text;
using System.Collections;
using UnityEngine;
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

    // Thanks to https://gist.github.com/tenpn/f8da1b7df7352a1d50ff for inspiration for this code.
    [PostProcessBuild(1400)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
#if UNITY_5_OR_NEWER
        if (target != BuildTarget.iOS && target != BuildTarget.tvOS && target != BuildTarget.WebGL) {
            return;
        }

        if (target == BuildTarget.WebGL) {

        // Read the index.html file and replace it line by line
        var indexPath = Path.Combine (path, "index.html");
        var indexLines = File.ReadAllLines  (indexPath);
        var sbWeb = new StringBuilder ();
        foreach (var line in indexLines) {
            sbWeb.AppendLine (line);
        }
        File.WriteAllText(indexPath, sbWeb.ToString());
        return;
    }
#else
    if (target != BuildTarget.iPhone) {
        return;
    }
#endif

        var scriptUUID = getUUIDForPbxproj ();

        var xcodeProjectPath = Path.Combine (path, "Unity-iPhone.xcodeproj");
        var pbxPath = Path.Combine (xcodeProjectPath, "project.pbxproj");

        var output = new StringBuilder();
        var xcodeProjectLines = File.ReadAllLines(pbxPath).GetEnumerator();

        while (xcodeProjectLines.MoveNext())
        {
            var currentLine = (string)xcodeProjectLines.Current;

            if (currentLine.Contains("GCC_ENABLE_OBJC_EXCEPTIONS"))
            {
                output.AppendLine(currentLine.Replace("NO", "YES"));
            }
            else if (currentLine.Contains("GCC_ENABLE_CPP_EXCEPTIONS"))
            {
                output.AppendLine(currentLine.Replace("NO", "YES"));
            }
            else if (currentLine.Contains("/* Begin PBXResourcesBuildPhase section */"))
            {
                output.AppendLine(currentLine);
                ProcessResourcesBuildPhaseSection(xcodeProjectLines, scriptUUID, output);
            }
            else if (currentLine.Contains("buildPhases = ("))
            {
                output.AppendLine(currentLine);
                ProcessBuildPhase(xcodeProjectLines, scriptUUID, output);
            }
            else if (currentLine.Contains("OTHER_LDFLAGS = ("))
            {
                output.AppendLine(currentLine);
                ProcessLinkerFlags(xcodeProjectLines, output);
            }
            else
            {
                output.AppendLine(currentLine);
            }
        }

        File.WriteAllText(pbxPath, output.ToString());
    }

    static void ProcessBuildPhase(IEnumerator lines, string uuid, StringBuilder output)
    {
        var needsBuildPhase = true;

        while (lines.MoveNext())
        {
            var currentLine = (string)lines.Current;

            if (currentLine.Contains(uuid))
            {
                needsBuildPhase = false;
                output.AppendLine(currentLine);
            }
            else if (currentLine.Contains(");"))
            {
                if (needsBuildPhase)
                {
                    output.AppendFormat("\t\t\t\t{0} /* ShellScript */,", uuid);
                    output.AppendLine();
                }

                output.AppendLine(currentLine);
                break;
            }
            else
            {
                output.AppendLine(currentLine);
            }
        }
    }

    static void ProcessResourcesBuildPhaseSection(IEnumerator lines, string uuid, StringBuilder output)
    {
        var needsBuildPhaseScript = true;

        while (lines.MoveNext())
        {
            var currentLine = (string)lines.Current;

            if (currentLine.Contains("bugsnag dsym upload script"))
            {
                needsBuildPhaseScript = false;
                output.AppendLine(currentLine);
            }
            else if (currentLine.Contains("/* End PBXResourcesBuildPhase section */"))
            {
                if (needsBuildPhaseScript)
                {
                    output.AppendFormat("\t\t{0} /* ShellScript */ = {{", uuid);
                    output.AppendLine();
                    output.AppendLine("\t\t\tisa = PBXShellScriptBuildPhase;");
                    output.AppendLine("\t\t\tbuildActionMask = 2147483647;");
                    output.AppendLine("\t\t\tfiles = (");
                    output.AppendLine("\t\t\t);");
                    output.AppendLine("\t\t\tinputPaths = (");
                    output.AppendLine("\t\t\t);");
                    output.AppendLine("\t\t\toutputPaths = (");
                    output.AppendLine("\t\t\t);");
                    output.AppendLine("\t\t\trunOnlyForDeploymentPostprocessing = 0;");
                    output.AppendLine("\t\t\tshellPath = \"/usr/bin/env ruby\";");
                    output.AppendLine("\t\t\tshellScript = \"# bugsnag dsym upload script\\nfork do\\n  Process.setsid\\n  STDIN.reopen(\\\"/dev/null\\\")\\n  STDOUT.reopen(\\\"/dev/null\\\", \\\"a\\\")\\n  STDERR.reopen(\\\"/dev/null\\\", \\\"a\\\")\\n\\n  require \\\"shellwords\\\"\\n\\n  Dir[\\\"#{ENV[\\\"DWARF_DSYM_FOLDER_PATH\\\"]}/*/Contents/Resources/DWARF/*\\\"].each do |dsym|\\n    system(\\\"curl -F dsym=@#{Shellwords.escape(dsym)} -F projectRoot=#{Shellwords.escape(ENV[\\\"PROJECT_DIR\\\"])} https://upload.bugsnag.com/\\\")\\n  end\\nend\";");
                    output.AppendLine("\t\t};");
                }

                output.AppendLine(currentLine);
                break;
            }
            else
            {
                output.AppendLine(currentLine);
            }
        }
    }

    static void ProcessLinkerFlags(IEnumerator lines, StringBuilder output)
    {
        bool needsLinkerFlag = true;

        while (lines.MoveNext())
        {
            var currentLine = (string)lines.Current;

            if (currentLine.Contains("-ObjC"))
            {
                needsLinkerFlag = false;
                output.AppendLine(currentLine);
            }
            else if (currentLine.Contains(");"))
            {
                if (needsLinkerFlag)
                {
                    output.AppendLine("\t\t\t\t\t\"-ObjC\"");
                }

                output.AppendLine(currentLine);
                break;
            }
            else
            {
                output.AppendLine(currentLine);
            }
        }
    }

    static string getUUIDForPbxproj() {
        return Guid.NewGuid ().ToString ("N").Substring (0, 24).ToUpper ();
    }

  }
#endif
}
