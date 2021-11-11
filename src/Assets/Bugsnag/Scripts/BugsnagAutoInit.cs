using UnityEngine;

namespace BugsnagUnity.Editor
{

    public class BugsnagAutoInit
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoadRuntimeMethod()
        {
            var settings = Resources.Load<BugsnagSettingsObject>("BugsnagSettings");
            Bugsnag.Start(settings.GetConfig());
        }
    }

}