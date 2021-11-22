using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using BugsnagUnity;
using UnityEditor.Callbacks;
using System.Linq;

namespace BugsnagUnity.Editor
{
    public class BugsnagEditor : EditorWindow
    {

        private bool _showAdvancedSettings;

        public Texture IconTexture, LogoTexture;

        private void OnEnable()
        {
            titleContent.image = IconTexture;
            titleContent.text = "Bugsnag";
        }

        [MenuItem("Window/Bugsnag/Settings")]
        public static void ShowWindow()
        {
            GetWindow(typeof(BugsnagEditor));
        }

        private static bool SettingsFileFound()
        {
            return File.Exists(Application.dataPath + "/Resources/Bugsnag/BugsnagSettingsObject.asset");
        }

        void OnGUI()
        {
            DrawLogo();
            if (!SettingsFileFound())
            {
                CreateNewSettingsFile();
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

        private void DrawSettingsEditorWindow()
        {
            GUILayout.Label("Bugsnag settings", EditorStyles.largeLabel);
            GUILayout.Space(5);
            EditorGUI.indentLevel++;

            var settings = GetSettingsObject();
            settings.AutoStartBugsnag = EditorGUILayout.Toggle("Auto Start Bugsnag",settings.AutoStartBugsnag);
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
            EditorGUIUtility.labelWidth = 200;

            SerializedObject so = new SerializedObject(settings);

            EditorGUILayout.PropertyField(so.FindProperty("AppType"));
            EditorGUILayout.PropertyField(so.FindProperty("AppHangThresholdMillis"));
            EditorGUILayout.PropertyField(so.FindProperty("AppVersion"));
            EditorGUILayout.PropertyField(so.FindProperty("BundleVersion"));
            EditorGUILayout.PropertyField(so.FindProperty("BreadcrumbLogLevel"));
            EditorGUILayout.PropertyField(so.FindProperty("Context"));
            EditorGUILayout.PropertyField(so.FindProperty("DiscardClasses"));
            EditorGUILayout.PropertyField(so.FindProperty("EnabledReleaseStages"));
            EditorGUILayout.PropertyField(so.FindProperty("EnabledErrorTypes"));
            EditorGUILayout.PropertyField(so.FindProperty("EnabledBreadcrumbTypes"));
            EditorGUILayout.PropertyField(so.FindProperty("LaunchDurationMillis"));
            EditorGUILayout.PropertyField(so.FindProperty("MaximumBreadcrumbs"));
            EditorGUILayout.PropertyField(so.FindProperty("NotifyEndpoint"));
            EditorGUILayout.PropertyField(so.FindProperty("SessionEndpoint"));
            EditorGUILayout.PropertyField(so.FindProperty("RedactedKeys"));
            EditorGUILayout.PropertyField(so.FindProperty("ReleaseStage"));

            EditorGUIUtility.labelWidth = 270;
            EditorGUILayout.PropertyField(so.FindProperty("ReportUncaughtExceptionsAsHandled"));
            EditorGUILayout.PropertyField(so.FindProperty("SendLaunchCrashesSynchronously"));

            EditorGUIUtility.labelWidth = 200;
            EditorGUILayout.PropertyField(so.FindProperty("SecondsPerUniqueLog"));
            so.ApplyModifiedProperties();
            EditorGUI.indentLevel--;
            EditorGUIUtility.labelWidth = originalWidth;
        }

        private BugsnagSettingsObject GetSettingsObject()
        {
            return Resources.Load<BugsnagSettingsObject>("Bugsnag/BugsnagSettingsObject");
        }

        private void CreateNewSettingsFile()
        {
            var resPath = Application.dataPath + "/Resources/Bugsnag";
            Directory.CreateDirectory(resPath);
            var asset = CreateInstance<BugsnagSettingsObject>();
            AssetDatabase.CreateAsset(asset, "Assets/Resources/Bugsnag/BugsnagSettingsObject.asset");
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