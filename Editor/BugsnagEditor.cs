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

        private bool _showBasicConfig = true;

        private bool _showAdvancedSettings, _showAppInformation, _showEndpoints, _showEnabledErrorTypes;

        public Texture DarkIcon, LightIcon;

        private Vector2 _scrollPos;


        private void OnEnable()
        {
            titleContent.text = "Bugsnag";
        }

        [MenuItem("Window/Bugsnag/Configuration")]
        public static void ShowWindow()
        {
            GetWindow(typeof(BugsnagEditor));
        }

        private static bool SettingsFileFound()
        {
            return File.Exists(Application.dataPath + "/Resources/Bugsnag/BugsnagSettingsObject.asset");
        }

        private void OnGUI()
        {
            DrawIcon();
            if (!SettingsFileFound())
            {
                CreateNewSettingsFile();
            }
            else
            {
                DrawSettingsEditorWindow();
            }
        }

        private void DrawIcon()
        {
            titleContent.image = EditorGUIUtility.isProSkin ? LightIcon : DarkIcon;
        }

        private void DrawSettingsEditorWindow()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos,
                                                    false,
                                                    false);
            GUILayout.BeginVertical();

            GUILayout.Space(10);
            var settings = GetSettingsObject();
            var so = new SerializedObject(settings);


            _showBasicConfig = EditorGUILayout.Foldout(_showBasicConfig, "Basic Configuration", true);
            if (_showBasicConfig)
            {
                DrawBasicConfiguration(settings);
            }

            GUILayout.Space(5);
            _showAdvancedSettings = EditorGUILayout.Foldout(_showAdvancedSettings, "Advanced Configuration", true);
            if (_showAdvancedSettings)
            {
                DrawAdvancedSettings(so, settings);
            }

            GUILayout.Space(5);
            _showAppInformation = EditorGUILayout.Foldout(_showAppInformation, "App Information", true);
            if (_showAppInformation)
            {
                DrawAppInfo(so);
            }

            GUILayout.Space(5);
            _showEndpoints = EditorGUILayout.Foldout(_showEndpoints, "Endpoints", true);
            if (_showEndpoints)
            {
                DrawEndpoints(so);
            }
            GUILayout.Space(10);

            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(settings);
        }

        private void DrawBasicConfiguration(BugsnagSettingsObject settings)
        {
            EditorGUI.indentLevel++;
            var originalWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 70;
            settings.ApiKey = EditorGUILayout.TextField("API Key", settings.ApiKey);
            EditorGUIUtility.labelWidth = 280;
            settings.StartAutomaticallyAtLaunch = EditorGUILayout.Toggle("Start Automatically (requires API key to be set)", settings.StartAutomaticallyAtLaunch);
            EditorGUIUtility.labelWidth = originalWidth;
            EditorGUI.indentLevel--;
        }

        private void DrawEndpoints(SerializedObject so)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(so.FindProperty("NotifyEndpoint"));
            EditorGUILayout.PropertyField(so.FindProperty("SessionEndpoint"));
            EditorGUI.indentLevel--;
        }

        private void DrawAppInfo(SerializedObject so)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(so.FindProperty("AppType"));
            EditorGUILayout.PropertyField(so.FindProperty("AppVersion"));
            EditorGUILayout.PropertyField(so.FindProperty("ReleaseStage"));
            EditorGUILayout.LabelField("Android Only");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(so.FindProperty("BundleVersion"));
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("Cocoa Only");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(so.FindProperty("VersionCode"));
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }


        private void DrawAdvancedSettings(SerializedObject so, BugsnagSettingsObject settings)
        {
            EditorGUI.indentLevel++;
            var originalWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200;
            EditorGUILayout.PropertyField(so.FindProperty("AutoDetectErrors"));
            EditorGUILayout.PropertyField(so.FindProperty("AutoTrackSessions"));
            EditorGUILayout.PropertyField(so.FindProperty("AppHangThresholdMillis"));
            EditorGUILayout.PropertyField(so.FindProperty("BreadcrumbLogLevel"));
            EditorGUILayout.PropertyField(so.FindProperty("Context"));
            EditorGUILayout.PropertyField(so.FindProperty("LaunchDurationMillis"));
            EditorGUILayout.PropertyField(so.FindProperty("MaximumBreadcrumbs"));
            EditorGUILayout.PropertyField(so.FindProperty("MaxPersistedEvents"));
            EditorGUILayout.PropertyField(so.FindProperty("NotifyLogLevel"));
            EditorGUIUtility.labelWidth = 270;
            EditorGUILayout.PropertyField(so.FindProperty("ReportExceptionLogsAsHandled"));
            EditorGUILayout.PropertyField(so.FindProperty("SendLaunchCrashesSynchronously"));
            EditorGUIUtility.labelWidth = 200;
            EditorGUILayout.PropertyField(so.FindProperty("SecondsPerUniqueLog"));
            EditorGUILayout.PropertyField(so.FindProperty("PersistUser"));
            EditorGUILayout.PropertyField(so.FindProperty("SendThreads"));
            DrawEnabledErrorTypesDropdown(settings);
            EditorGUILayout.PropertyField(so.FindProperty("DiscardClasses"));
            EditorGUILayout.PropertyField(so.FindProperty("EnabledReleaseStages"));
            EditorGUILayout.PropertyField(so.FindProperty("EnabledBreadcrumbTypes"));
            EditorGUILayout.PropertyField(so.FindProperty("RedactedKeys"));


            EditorGUI.indentLevel--;
            EditorGUIUtility.labelWidth = originalWidth;

        }

        private void DrawEnabledErrorTypesDropdown(BugsnagSettingsObject settings)
        {
            _showEnabledErrorTypes = EditorGUILayout.Foldout(_showEnabledErrorTypes, "Enabled Error Types", true);
            if (_showEnabledErrorTypes)
            {
                EditorGUI.indentLevel += 2;
                settings.EnabledErrorTypes.ANRs = EditorGUILayout.Toggle("ANRs (Android)", settings.EnabledErrorTypes.ANRs);
                settings.EnabledErrorTypes.AppHangs = EditorGUILayout.Toggle("AppHangs (Cocoa)", settings.EnabledErrorTypes.AppHangs);
                settings.EnabledErrorTypes.OOMs = EditorGUILayout.Toggle("OOMs (Cocoa)", settings.EnabledErrorTypes.OOMs);
                settings.EnabledErrorTypes.Crashes = EditorGUILayout.Toggle("Crashes", settings.EnabledErrorTypes.Crashes);
                settings.EnabledErrorTypes.UnityLog = EditorGUILayout.Toggle("Unity Logs", settings.EnabledErrorTypes.UnityLog);
                EditorGUI.indentLevel -= 2;
            }
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