using UnityEditor;
using UnityEngine;

namespace BugsnagUnity.Editor
{

    public class AutoSymbolUploadToggleMenu
    {

        [MenuItem("Window/BugSnag/Auto Upload Symbols")]
        private static void ToggleAutoUpload()
        {
            SetEnabled(!IsEnabled());
        }

        [MenuItem("Window/BugSnag/Auto Upload Symbols", true)]
        private static bool ToggleAutoUploadValidate()
        {
            Menu.SetChecked("Window/BugSnag/Auto Upload Symbols", IsEnabled());
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