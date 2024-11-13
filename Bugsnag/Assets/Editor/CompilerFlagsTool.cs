using UnityEditor;

public static class CompilerFlagsMenu
{
    private static readonly string[] flags = { "BSG_COCOA_DEV", "BSG_WIN_DEV", "BSG_ANDROID_DEV" };

    [MenuItem("BugsnagDev/Select Env/COCOA")]
    private static void SetCocoaFlag()
    {
        SetCompilerFlag("BSG_COCOA_DEV");
    }

    [MenuItem("BugsnagDev/Select Env/WINDOWS")]
    private static void SetWinFlag()
    {
        SetCompilerFlag("BSG_WIN_DEV");
    }

    [MenuItem("BugsnagDev/Select Env/ANDROID")]
    private static void SetAndroidFlag()
    {
        SetCompilerFlag("BSG_ANDROID_DEV");
    }

    [MenuItem("BugsnagDev/Select Env/FALLBACK")]
    private static void SetNoFlag()
    {
        SetCompilerFlag(null);
    }

    private static void SetCompilerFlag(string selectedFlag)
    {
        try
        {
            string defines = selectedFlag ?? ""; // If selectedFlag is null, set defines to an empty string
            foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (group == BuildTargetGroup.Unknown) continue;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
            }
        }
        catch { }
    }
}