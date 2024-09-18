using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BugsnagUnity.Editor
{
    // This class contains all the code for the EDM4U support menu item. This is removed in UPM releases by the upm-tools/build-upm-package.sh script.
    public partial class BugsnagEditor : EditorWindow
    {

        // The kotlin Version here needs to match the one in the rake build file. Both should reflect what the android notifier is using.
        private const string ANDROID_DEPS_XML = "<dependencies><androidPackages><repositories><repository>https://mvnrepository.com/artifact/org.jetbrains.kotlin/kotlin-stdlib</repository></repositories><androidPackage spec=\"org.jetbrains.kotlin:kotlin-stdlib:1.4.32\"></androidPackage></androidPackages></dependencies>";

        private const string EDM_MENU_ITEM = "Window/BugSnag/Enable EDM4U Support";

        private static string EDMDepsFilePath = "/Bugsnag/Editor/BugsnagAndroidDependencies.xml";

        private static string KotlinLibsDirPath = "/Bugsnag/Plugins/Android/Kotlin";

        [MenuItem(EDM_MENU_ITEM, false, 1)]
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
            UnityEditor.Menu.SetChecked(EDM_MENU_ITEM, IsEDMEnabled());
            return true;
        }

        public static void EnableEDM()
        {
            try
            {
                EditDepsFile(true);
                UpdateKotlinLibraryImportSettings(false);
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError("Error enabling BugSnag EDM4U support: " + e.Message);
            }

            if (IsEDMEnabled())
            {
                ReportEDMSuccess("BugSnag EDM4U support successfully enabled.\n\nPlease restart Unity before building.");
            }
            else
            {
                ReportEDMError("Error enabling BugSnag EDM4U support.\n\nPlease check the console for error messages");
            }
        }

        public static void DisableEDM()
        {
            try
            {
                EditDepsFile(false);
                UpdateKotlinLibraryImportSettings(true);
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError("Error disabling BugSnag EDM4U support: " + e.Message);
            }

            if (!IsEDMEnabled())
            {
                ReportEDMSuccess("BugSnag EDM4U support successfully disabled.\n\nPlease restart Unity before building.");
            }
            else
            {
                ReportEDMError("Error disabling BugSnag EDM4U support.\n\nPlease check the console for error messages");
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

        private static void UpdateKotlinLibraryImportSettings(bool active)
        {
            foreach (var lib in GetKotlinLibs())
            {
                lib.SetCompatibleWithPlatform(BuildTarget.Android, active);
                lib.SaveAndReimport();
            }
        }

        private static void EditDepsFile(bool create)
        {
            var path = Application.dataPath + EDMDepsFilePath;
            if (create)
            {
                File.WriteAllText(path, ANDROID_DEPS_XML);
            }
            else
            {
                File.Delete(path);
                File.Delete(path + ".meta");
            }
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
            if (!File.Exists(Application.dataPath + EDMDepsFilePath))
            {
                return false;
            }
            foreach (var lib in GetKotlinLibs())
            {
                if (lib.GetCompatibleWithPlatform(BuildTarget.Android))
                {
                    return false;
                }
            }
            return true;
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
            EditorGUILayout.LabelField(_msg, style);
            GUILayout.Space(5);
            if (GUILayout.Button("ok")) this.Close();
        }
    }

}
