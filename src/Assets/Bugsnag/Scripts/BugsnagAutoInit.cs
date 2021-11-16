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
                var config = settings.GetConfig();
                config.ScriptingBackend = FindScriptingBackend();
                config.DotnetScriptingRuntime = FindDotnetScriptingRuntime();
                config.DotnetApiCompatibility = FindDotnetApiCompatibility();
                Bugsnag.Start(settings.GetConfig());
            }
        }

        private static string FindScriptingBackend()
        {
#if ENABLE_MONO
            return "Mono";
#elif ENABLE_IL2CPP
      return "IL2CPP";
#else
            return "Unknown";
#endif
        }

        private static string FindDotnetScriptingRuntime()
        {
#if NET_4_6
      return ".NET 4.6 equivalent";
#else
            return ".NET 3.5 equivalent";
#endif
        }

        private static string FindDotnetApiCompatibility()
        {
#if NET_2_0_SUBSET
      return ".NET 2.0 Subset";
#else
            return ".NET 2.0";
#endif
        }
    }

}