using UnityEditor;
using UnityEngine;
[InitializeOnLoad]
public class BugsnagAddScriptingSymbol : MonoBehaviour
{
    private const string DEFINE_SYMBOL = "BUGSNAG_UNITY_WEB_REQUEST";

    private static BuildTargetGroup[] _supportedPlatforms = { BuildTargetGroup.Android, BuildTargetGroup.Standalone, BuildTargetGroup.iOS, BuildTargetGroup.WebGL };

    static BugsnagAddScriptingSymbol()
    {
        foreach (var target in _supportedPlatforms)
        {
            try
            {
                SetScriptingSymbol(target);
            }
            catch
            {
                // Some users might not have a platform installed, in that case ignore the error
            }
        }
    }

    static void SetScriptingSymbol(BuildTargetGroup buildTargetGroup)
    {
        var existingSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        if (string.IsNullOrEmpty(existingSymbols))
        {
            existingSymbols = DEFINE_SYMBOL;
        }
        else if (!existingSymbols.Contains(DEFINE_SYMBOL))
        {
            existingSymbols += ";" + DEFINE_SYMBOL;
        }
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, existingSymbols);
    }
}
