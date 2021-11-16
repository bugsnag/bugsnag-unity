using UnityEngine;

namespace BugsnagUnity.Editor
{

    public class BugsnagAutoInit
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoadRuntimeMethod()
        {
            var settings = Resources.Load<BugsnagSettingsObject>("Bugsnag/BugsnagSettings");
            if (settings != null && !string.IsNullOrEmpty(settings.ApiKey))
            {
                Bugsnag.Start(settings.GetConfig());
            }
        }
    }

}