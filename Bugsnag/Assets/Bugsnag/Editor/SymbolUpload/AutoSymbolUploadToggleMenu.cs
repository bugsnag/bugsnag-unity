using UnityEditor;
using UnityEngine;

namespace BugsnagUnity.Editor
{

    public class AutoSymbolUploadToggleMenu
    {
        private const string MENU_PATH = "Window/BugSnag/Enable Symbol Uploads";

        [MenuItem(MENU_PATH,false,50)]
        private static void ToggleAutoUpload()
        {
            SetEnabled(!IsEnabled());
        }

        [MenuItem(MENU_PATH, true)]
        private static bool ToggleAutoUploadValidate()
        {
            Menu.SetChecked(MENU_PATH, IsEnabled());
            return true;
        }

        static bool IsEnabled()
        {   
            var settings = GetSettingsObject();
            return settings != null && settings.AutoUploadSymbols;
        }

        static void SetEnabled(bool enabled)
        {
            var settings = GetSettingsObject();
            if(settings != null)
            {
                settings.AutoUploadSymbols = enabled;
                EditorUtility.SetDirty(settings);
            }
        }

        private static BugsnagSettingsObject GetSettingsObject()
        {
            var config = Resources.Load<BugsnagSettingsObject>("Bugsnag/BugsnagSettingsObject");
            return config;
        }
    }
}