using UnityEditor;

namespace BugsnagUnity.Editor
{
    internal static class BugsnagPlayerSettingsCompat
    {
        // Get Scripting Backend
        public static ScriptingImplementation GetScriptingBackend(BuildTargetGroup buildTargetGroup)
        {
#if UNITY_6000_0_OR_NEWER
        return PlayerSettings.GetScriptingBackend(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup));
#else
            return PlayerSettings.GetScriptingBackend(buildTargetGroup);
#endif
        }

        // Get Scripting Define Symbols
        public static string GetScriptingDefineSymbols(BuildTargetGroup buildTargetGroup)
        {
#if UNITY_6000_0_OR_NEWER
        return PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup));
#else
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
#endif
        }

        // Set Scripting Define Symbols
        public static void SetScriptingDefineSymbols(BuildTargetGroup buildTargetGroup, string defineSymbols)
        {
#if UNITY_6000_0_OR_NEWER
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup), defineSymbols);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defineSymbols);
#endif
        }
    }
}