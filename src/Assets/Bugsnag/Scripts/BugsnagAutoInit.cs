using UnityEngine;

namespace BugsnagUnity.Editor
{

    public class BugsnagAutoInit
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoadRuntimeMethod()
        {
            var settings = Resources.Load<BugsnagSettingsObject>("BugsnagSettings");
            if (settings != null)
            {
                Bugsnag.Start(settings.GetConfig());
            }
            else
            {
                Debug.LogWarning("No Bugsnag settings object found, Bugsnag will not be automatically started. Please create one via the Bugsnag>Settings menu item if you want to use the automatic start feature.");
            }
        }
    }

}