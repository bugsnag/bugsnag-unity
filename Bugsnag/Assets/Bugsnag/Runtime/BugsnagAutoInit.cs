using UnityEngine;
namespace BugsnagUnity
{

    public class BugsnagAutoInit
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnBeforeSceneLoadRuntimeMethod()
        {
            var settings = Resources.Load<BugsnagSettingsObject>("Bugsnag/BugsnagSettingsObject");
            if (settings != null && settings.StartAutomaticallyAtLaunch)
            {
                if (string.IsNullOrEmpty(settings.ApiKey))
                {
                    Debug.LogError("Bugsnag not auto started as the API key is not set in the Bugsnag Settings window.");
                    return;
                }
                var config = settings.GetConfig();
                Bugsnag.Start(config);
            }
        }

    }

}