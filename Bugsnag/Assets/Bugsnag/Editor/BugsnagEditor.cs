using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using BugsnagUnity;
using UnityEditor.Callbacks;
using System.Linq;
using System;

namespace BugsnagUnity.Editor
{
    public partial class BugsnagEditor : EditorWindow
    {

        private bool _showBasicConfig = true;

        private bool _showAdvancedSettings, _showAppInformation, _showEndpoints, _showEnabledErrorTypes, _showSwitch;

        public Texture DarkIcon, LightIcon;

        private Vector2 _scrollPos;


        private void OnEnable()
        {
            titleContent.text = "BugSnag";
            CheckForSettingsCreation();
        }

        [MenuItem("Window/BugSnag/Configuration", false, 0)]
        public static void ShowWindow()
        {
            CheckForSettingsCreation();
            GetWindow(typeof(BugsnagEditor));
        }

        private void Update()
        {
            CheckForSettingsCreation();
        }

        private void OnGUI()
        {
            DrawIcon();
            if (SettingsFileFound())
            {
                DrawSettingsEditorWindow();
            }
        }

        private static bool SettingsFileFound()
        {
            return File.Exists(Application.dataPath + "/Resources/Bugsnag/BugsnagSettingsObject.asset");
        }

        private static void CheckForSettingsCreation()
        {
            if (!SettingsFileFound())
            {
                CreateNewSettingsFile();
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

            var assemblyName = BugsnagUnity.Configuration.GetAssemblyName();
            var version = assemblyName.Version;
            EditorGUILayout.LabelField($"BugSnag Unity version {version.Major}.{version.Minor}.{version.Build}");

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
                DrawAppInfo(so, settings);
            }

            GUILayout.Space(5);
            _showEndpoints = EditorGUILayout.Foldout(_showEndpoints, "Endpoints", true);
            if (_showEndpoints)
            {
                DrawEndpoints(so);
            }

            GUILayout.Space(5);
            _showSwitch = EditorGUILayout.Foldout(_showSwitch, new GUIContent("Nintendo Switch ⓘ", "Requires Nintendo Switch Bugsnag plugin"), true);
            if (_showSwitch)
            {
                DrawSwitchOptions(so);
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

        private void DrawAppInfo(SerializedObject so, BugsnagSettingsObject settings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(so.FindProperty("AppType"));
            EditorGUILayout.PropertyField(so.FindProperty("AppVersion"));
            EditorGUILayout.PropertyField(so.FindProperty("ReleaseStage"));
            settings.VersionCode = EditorGUILayout.IntField(new GUIContent("Version Code ⓘ", "Android devices only"), settings.VersionCode);
            settings.BundleVersion = EditorGUILayout.TextField(new GUIContent("Bundle Version ⓘ", "Apple devices only"), settings.BundleVersion);
            EditorGUI.indentLevel--;
        }

        private void DrawAdvancedSettings(SerializedObject so, BugsnagSettingsObject settings)
        {
            EditorGUI.indentLevel++;
            var originalWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 270;
            var appHangThresholdMillisValue = EditorGUILayout.LongField(new GUIContent("App Hang Threshold Millis ⓘ", "Apple devices only"), (long)settings.AppHangThresholdMillis);
            if (appHangThresholdMillisValue >= 0)
            {
                settings.AppHangThresholdMillis = (ulong)appHangThresholdMillisValue;
            }
            EditorGUILayout.PropertyField(so.FindProperty("AutoDetectErrors"));
            EditorGUILayout.PropertyField(so.FindProperty("AutoTrackSessions"));
            EditorGUILayout.PropertyField(so.FindProperty("GenerateAnonymousId"));
            EditorGUILayout.PropertyField(so.FindProperty("BreadcrumbLogLevel"));
            EditorGUILayout.PropertyField(so.FindProperty("Context"));
            EditorGUILayout.PropertyField(so.FindProperty("DiscardClasses"));
            EditorGUILayout.PropertyField(so.FindProperty("EnabledBreadcrumbTypes"));
            DrawEnabledErrorTypesDropdown(settings);
            EditorGUILayout.PropertyField(so.FindProperty("EnabledReleaseStages"));
            EditorGUILayout.PropertyField(so.FindProperty("LaunchDurationMillis"));
            settings.MaximumBreadcrumbs = EditorGUILayout.IntField("Max Breadcrumbs", settings.MaximumBreadcrumbs);
            EditorGUILayout.PropertyField(so.FindProperty("MaxPersistedEvents"));
            EditorGUILayout.PropertyField(so.FindProperty("MaxPersistedSessions"));
            settings.MaxReportedThreads = EditorGUILayout.IntField(new GUIContent("Max Reported Threads ⓘ", "Android devices only"), settings.MaxReportedThreads);
            EditorGUILayout.PropertyField(so.FindProperty("MaxStringValueLength"));
            EditorGUILayout.PropertyField(so.FindProperty("NotifyLogLevel"));
            EditorGUILayout.PropertyField(so.FindProperty("PersistUser"));
            EditorGUILayout.PropertyField(so.FindProperty("RedactedKeys"));
            EditorGUILayout.PropertyField(so.FindProperty("ReportExceptionLogsAsHandled"));
            EditorGUILayout.PropertyField(so.FindProperty("SecondsPerUniqueLog"));
            EditorGUILayout.PropertyField(so.FindProperty("SendLaunchCrashesSynchronously"));
            EditorGUILayout.PropertyField(so.FindProperty("SendThreads"));
            EditorGUILayout.PropertyField(so.FindProperty("Telemetry"));
            EditorGUIUtility.labelWidth = originalWidth;
            EditorGUI.indentLevel--;

        }

        private void DrawSwitchOptions(SerializedObject so)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(so.FindProperty("SwitchCacheIndex"));
            EditorGUILayout.PropertyField(so.FindProperty("SwitchCacheMaxSize"));
            EditorGUILayout.PropertyField(so.FindProperty("SwitchCacheMountName"));
            EditorGUILayout.PropertyField(so.FindProperty("SwitchCacheType"));
            EditorGUI.indentLevel--;
        }

        private void DrawEnabledErrorTypesDropdown(BugsnagSettingsObject settings)
        {

            var style = new GUIStyle(GUI.skin.GetStyle("foldout"));
            style.margin = new RectOffset(2, 0, 0, 0);
            _showEnabledErrorTypes = EditorGUILayout.Foldout(_showEnabledErrorTypes, "Enabled Error Types", true, style);
            if (_showEnabledErrorTypes)
            {
                EditorGUI.indentLevel += 2;
                settings.EnabledErrorTypes.ANRs = EditorGUILayout.Toggle(new GUIContent("ANRs ⓘ", "Android devices only"), settings.EnabledErrorTypes.ANRs);
                settings.EnabledErrorTypes.AppHangs = EditorGUILayout.Toggle(new GUIContent("App Hangs ⓘ", "Apple devices only"), settings.EnabledErrorTypes.AppHangs);
                settings.EnabledErrorTypes.Crashes = EditorGUILayout.Toggle(new GUIContent("Crashes ⓘ", "Android and Apple devices only"), settings.EnabledErrorTypes.Crashes);
                settings.EnabledErrorTypes.OOMs = EditorGUILayout.Toggle(new GUIContent("OOMs ⓘ", "iOS devices only"), settings.EnabledErrorTypes.OOMs);
                settings.EnabledErrorTypes.ThermalKills = EditorGUILayout.Toggle(new GUIContent("Thermal Kills ⓘ", "iOS devices only"), settings.EnabledErrorTypes.ThermalKills);
                settings.EnabledErrorTypes.UnityLog = EditorGUILayout.Toggle("Unity Logs", settings.EnabledErrorTypes.UnityLog);
                EditorGUI.indentLevel -= 2;
            }
        }

        private BugsnagSettingsObject GetSettingsObject()
        {
            return Resources.Load<BugsnagSettingsObject>("Bugsnag/BugsnagSettingsObject");
        }

        private static void CreateNewSettingsFile()
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