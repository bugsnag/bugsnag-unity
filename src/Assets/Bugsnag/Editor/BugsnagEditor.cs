using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using BugsnagUnity.Payload;
using UnityEditor.Callbacks;
using System.Linq;

namespace BugsnagUnity.Editor
{
    public class BugsnagEditor : EditorWindow
    {

        private bool _showEnabledErrorTypes;
        private bool _showEnabledBreadcrumbTypes;
        private bool _showAdvancedSettings;

        public Texture IconTexture, LogoTexture;

        private void OnEnable()
        {
            titleContent.image = IconTexture;
            titleContent.text = "Bugsnag";
        }

        [MenuItem("Bugsnag/Settings")]
        public static void ShowWindow()
        {
            GetWindow(typeof(BugsnagEditor));
        }

        private static bool SettingsFileFound()
        {
            return File.Exists(Application.dataPath + "/Resources/Bugsnag/BugsnagSettings.asset");
        }

        void OnGUI()
        {
            DrawLogo();
            if (!SettingsFileFound())
            {
                DrawSettingsCreationWindow();
            }
            else
            {
                DrawSettingsEditorWindow();
            }
        }

        private void DrawLogo()
        {
            var bgTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            var c = Color.white;
            c.a = 0.5f;
            bgTex.SetPixel(0, 0, c);
            bgTex.Apply();
            GUI.DrawTexture(new Rect(0, 0, maxSize.x, 58), bgTex, ScaleMode.StretchToFill);

            GUI.DrawTexture(new Rect(5, 5, 125, 46), LogoTexture, ScaleMode.ScaleToFit);
            GUILayout.Space(70);

        }

        private void DrawSettingsCreationWindow()
        {
            GUIStyle wordWrap = EditorStyles.largeLabel;
            wordWrap.wordWrap = true;
            GUILayout.Label("No Bugsnag Settings File Found.", wordWrap);
            GUILayout.Label("Please create one if you want Bugsnag to start automatically.", wordWrap);
            GUILayout.Space(5);

            if (GUILayout.Button("Create New Settings File"))
            {
                CreateNewSettingsFile();
            }
        }

        private void DrawSettingsEditorWindow()
        {

            GUILayout.Label("Bugsnag settings", EditorStyles.largeLabel);
            GUILayout.Space(5);

            EditorGUI.indentLevel++;

            var settings = GetSettingsObject();
            settings.ApiKey = EditorGUILayout.TextField("API Key", settings.ApiKey);
            settings.AutoDetectErrors = EditorGUILayout.Toggle("Auto Detect Errors", settings.AutoDetectErrors);
            settings.AutoTrackSessions = EditorGUILayout.Toggle("Auto Track Sessions", settings.AutoTrackSessions);
            settings.NotifyLogLevel = (LogType)EditorGUILayout.EnumPopup("Notify Log Level", settings.NotifyLogLevel);

            GUILayout.Space(5);

            _showAdvancedSettings = EditorGUILayout.Foldout(_showAdvancedSettings, "Advanced Settings", true);

            if (_showAdvancedSettings)
            {
                DrawAdvancedSettings(settings);
            }

            EditorGUI.indentLevel--;
            EditorUtility.SetDirty(settings);

        }

        private void DrawAdvancedSettings(BugsnagSettingsObject settings)
        {
            EditorGUI.indentLevel++;
            var originalWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 175;

            settings.SecondsPerUniqueLog = EditorGUILayout.DoubleField("Seconds Per Unique Log", settings.SecondsPerUniqueLog);
            settings.MaximumBreadcrumbs = EditorGUILayout.IntSlider("Maximum Breadcrumbs", settings.MaximumBreadcrumbs, 0, 100);

            DrawEnabledErrorTypesDropdown(settings);

            DrawEnabledBreadcrumbTypesDropdown(settings);

            EditorGUI.indentLevel--;
            EditorGUIUtility.labelWidth = originalWidth;
        }

        private void DrawEnabledErrorTypesDropdown(BugsnagSettingsObject settings)
        {
            _showEnabledErrorTypes = EditorGUILayout.Foldout(_showEnabledErrorTypes, "Enabled Error Types", true);
            if (_showEnabledErrorTypes)
            {
                EditorGUI.indentLevel += 2;
                settings.EnabledErrorTypes.ANRs = EditorGUILayout.Toggle("ANRs", settings.EnabledErrorTypes.ANRs);
                settings.EnabledErrorTypes.AppHangs = EditorGUILayout.Toggle("AppHangs", settings.EnabledErrorTypes.AppHangs);
                settings.EnabledErrorTypes.OOMs = EditorGUILayout.Toggle("OOMs", settings.EnabledErrorTypes.OOMs);
                settings.EnabledErrorTypes.NativeCrashes = EditorGUILayout.Toggle("Native Crashes", settings.EnabledErrorTypes.NativeCrashes);
                settings.EnabledErrorTypes.UnhandledExceptions = EditorGUILayout.Toggle("Unhandled Exceptions", settings.EnabledErrorTypes.UnhandledExceptions);
                settings.EnabledErrorTypes.UnityLogLogs = EditorGUILayout.Toggle("Unity.Log Logs", settings.EnabledErrorTypes.UnityLogLogs);
                settings.EnabledErrorTypes.UnityAssertLogs = EditorGUILayout.Toggle("Unity.Assert Logs", settings.EnabledErrorTypes.UnityAssertLogs);
                settings.EnabledErrorTypes.UnityWarningLogs = EditorGUILayout.Toggle("Unity.Warning Logs", settings.EnabledErrorTypes.UnityWarningLogs);
                settings.EnabledErrorTypes.UnityErrorLogs = EditorGUILayout.Toggle("Unity.Error Logs", settings.EnabledErrorTypes.UnityErrorLogs);
                EditorGUI.indentLevel -= 2;
            }
        }

        private void DrawEnabledBreadcrumbTypesDropdown(BugsnagSettingsObject settings)
        {
            _showEnabledBreadcrumbTypes = EditorGUILayout.Foldout(_showEnabledBreadcrumbTypes, "Enabled Breadcrumb Types", true);
            if (_showEnabledBreadcrumbTypes)
            {
                EditorGUI.indentLevel += 2;
                settings.EnabledBreadcrumbTypes.Navigation = EditorGUILayout.Toggle("Navigation", settings.EnabledBreadcrumbTypes.Navigation);
                settings.EnabledBreadcrumbTypes.Request = EditorGUILayout.Toggle("Request", settings.EnabledBreadcrumbTypes.Request);
                settings.EnabledBreadcrumbTypes.Process = EditorGUILayout.Toggle("Process", settings.EnabledBreadcrumbTypes.Process);
                settings.EnabledBreadcrumbTypes.Log = EditorGUILayout.Toggle("Log", settings.EnabledBreadcrumbTypes.Log);
                settings.EnabledBreadcrumbTypes.User = EditorGUILayout.Toggle("User", settings.EnabledBreadcrumbTypes.User);
                settings.EnabledBreadcrumbTypes.State = EditorGUILayout.Toggle("State", settings.EnabledBreadcrumbTypes.State);
                settings.EnabledBreadcrumbTypes.Error = EditorGUILayout.Toggle("Error", settings.EnabledBreadcrumbTypes.Error);
                settings.EnabledBreadcrumbTypes.Manual = EditorGUILayout.Toggle("Manual", settings.EnabledBreadcrumbTypes.Manual);
                EditorGUI.indentLevel -= 2;
            }
        }

        private BugsnagSettingsObject GetSettingsObject()
        {
            return Resources.Load<BugsnagSettingsObject>("Bugsnag/BugsnagSettings");
        }

        private void CreateNewSettingsFile()
        {
            var resPath = Application.dataPath + "/Resources/Bugsnag";
            Directory.CreateDirectory(resPath);
            var asset = CreateInstance<BugsnagSettingsObject>();
            AssetDatabase.CreateAsset(asset, "Assets/Resources/Bugsnag/BugsnagSettings.asset");
            AssetDatabase.SaveAssets();
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
}