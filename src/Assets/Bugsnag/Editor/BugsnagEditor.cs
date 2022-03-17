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
    public class BugsnagEditor : EditorWindow
    {

        private const string ANDROID_DEPS_XML = "<dependencies><androidPackages><repositories><repository>https://mvnrepository.com/artifact/org.jetbrains.kotlin/kotlin-stdlib</repository></repositories><androidPackage spec=\"org.jetbrains.kotlin:kotlin-stdlib:1.5.0\"></androidPackage></androidPackages></dependencies>";

        private const string EDM_MENU_ITEM = "Window/Bugsnag/Enable EDM4U Support";

        private static string EDMDepsFilePath = "/Bugsnag/Editor/BugsnagAndroidDependencies.xml";

        private static string KotlinLibsDirPath = "/Bugsnag/Plugins/Android/Kotlin";

        private bool _showBasicConfig = true;

        private bool _showAdvancedSettings, _showAppInformation, _showEndpoints, _showEnabledErrorTypes;

        public Texture DarkIcon, LightIcon;

        private Vector2 _scrollPos;


        [MenuItem(EDM_MENU_ITEM,false,1)]
        private static void ToggleEDM()
        {
            if (IsEDMEnabled())
            {
                DisableEDM();
            }
            else
            {
                EnableEDM();
            }
        }

        [MenuItem(EDM_MENU_ITEM, true)]
        private static bool ToggleEDMValidate()
        {
            Menu.SetChecked(EDM_MENU_ITEM, IsEDMEnabled());
            return true;
        }

        private void OnEnable()
        {
            titleContent.text = "Bugsnag";
            CheckForSettingsCreation();
        }

        [MenuItem("Window/Bugsnag/Configuration",false,0)]
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
            EditorGUILayout.PropertyField(so.FindProperty("NotifyLogLevel"));
            EditorGUILayout.PropertyField(so.FindProperty("PersistUser"));
            EditorGUILayout.PropertyField(so.FindProperty("RedactedKeys"));
            EditorGUILayout.PropertyField(so.FindProperty("ReportExceptionLogsAsHandled"));
            EditorGUILayout.PropertyField(so.FindProperty("SecondsPerUniqueLog"));
            EditorGUILayout.PropertyField(so.FindProperty("SendLaunchCrashesSynchronously"));
            EditorGUILayout.PropertyField(so.FindProperty("SendThreads"));
            EditorGUIUtility.labelWidth = originalWidth;
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

        private static void EnableEDM()
        {
            try
            {
                var path = Application.dataPath + EDMDepsFilePath;

                File.WriteAllText(path, ANDROID_DEPS_XML);

                foreach (var lib in GetKotlinLibs())
                {
                    lib.SetCompatibleWithPlatform(BuildTarget.Android, false);
                    lib.SaveAndReimport();
                }

                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError("Error enabling Bugsnag EDM4U support: " + e.Message);
            }

            if (IsEDMEnabled())
            {
                ReportEDMSuccess("Bugsnag EDM4U support successfully enabled.\n\nPlease restart Unity before building.");
            }
            else
            {
                ReportEDMError("Error enabling Bugsnag EDM4U support.\n\nPlease check the console for error messages");
            }
        }

        private static void DisableEDM()
        {
            try
            {
                var path = Application.dataPath + EDMDepsFilePath;
                File.Delete(path);
                File.Delete(path + ".meta");

                foreach (var lib in GetKotlinLibs())
                {
                    lib.SetCompatibleWithPlatform(BuildTarget.Android, true);
                    lib.SaveAndReimport();
                }

                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError("Error disabling Bugsnag EDM4U support: " + e.Message);
            }

            if (!IsEDMEnabled())
            {
                ReportEDMSuccess("Bugsnag EDM4U support successfully disabled.\n\nPlease restart Unity before building.");
            }
            else
            {
                ReportEDMError("Error disabling Bugsnag EDM4U support.\n\nPlease check the console for error messages");
            }
        }

        private static void ReportEDMSuccess(string msg)
        {
            BugsnagEDMPopup.Open(msg);
            Debug.Log(msg);
        }

        private static void ReportEDMError(string msg)
        {
            BugsnagEDMPopup.Open(msg);
            Debug.LogError(msg);
        }

        private static List<PluginImporter> GetKotlinLibs()
        {
            var kotlinLibs = new List<PluginImporter>();
            foreach (var libPath in Directory.GetFiles(Application.dataPath + KotlinLibsDirPath, "*.jar"))
            {
                kotlinLibs.Add((PluginImporter)AssetImporter.GetAtPath(libPath.Replace(Application.dataPath, "Assets")));
            }
            return kotlinLibs;
        }

        private static bool IsEDMEnabled()
        {
            var success = File.Exists(Application.dataPath + EDMDepsFilePath);
            foreach (var lib in GetKotlinLibs())
            {
                if (lib.GetCompatibleWithPlatform(BuildTarget.Android))
                {
                    success = false;
                }
            }
            return success;
        }
    }

    public class BugsnagEDMPopup : EditorWindow
    {
        private static string _msg;

        public static void Open(string msg)
        {
            _msg = msg;
            BugsnagEDMPopup window = ScriptableObject.CreateInstance<BugsnagEDMPopup>();
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 200, 110);
            window.ShowPopup();
        }

        void OnGUI()
        {
            var style = EditorStyles.wordWrappedLabel;
            style.alignment = TextAnchor.MiddleCenter;
            GUILayout.Space(5);
            EditorGUILayout.LabelField(_msg,style );
            GUILayout.Space(5);
            if (GUILayout.Button("ok")) this.Close();
        }
    }
}