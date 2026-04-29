using System.Linq;
using UnityEngine;
using UnityEditor;

public class ExampleAppBuilder
{
    public static void AndroidRelease()
    {
        BuildAndroid(false);
    }

    public static void AndroidDev()
    {
        BuildAndroid(true);
    }

    static void BuildAndroid(bool dev)
    {
        Debug.Log("Building Example Android app...");

        // Ensure an APK is produced (not an AAB) and avoid requiring a custom keystore on CI.
        EditorUserBuildSettings.buildAppBundle = false;
        PlayerSettings.Android.useCustomKeystore = false;
        PlayerSettings.Android.keystoreName = string.Empty;
        PlayerSettings.Android.keyaliasName = string.Empty;
        
        // Set scripting backend to IL2CPP (required for ARM64 and native libraries)
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.bugsnag.unity.example.android");

        // Set minSdkVersion to help avoid D8 issues
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        
        var opts = CommonMobileBuildOptions(dev ? "example_dev.apk" : "example.apk", dev);
        opts.target = BuildTarget.Android;

#if UNITY_2022_1_OR_NEWER
        PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;
#endif

        var result = BuildPipeline.BuildPlayer(opts);
        Debug.Log("Build Result: " + result);
    }

    public static void IosRelease()
    {
        IosBuild(false);
    }

    public static void IosDev()
    {
        IosBuild(true);
    }

    static void IosBuild(bool dev)
    {
        Debug.Log("Building Example iOS app...");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.bugsnag.unity.example.ios");

    #if UNITY_2022_1_OR_NEWER
        PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;
    #else
        PlayerSettings.iOS.allowHTTPDownload = true;
    #endif
        
        var opts = CommonMobileBuildOptions(dev ? "example_dev_xcode" : "example_xcode", dev);
        opts.target = BuildTarget.iOS;
        
        var result = BuildPipeline.BuildPlayer(opts);
        Debug.Log("Build Result: " + result);
    }

    private static BuildPlayerOptions CommonMobileBuildOptions(string outputFile, bool dev)
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (scenes.Length == 0)
        {
            // Some CI-created projects won't have EditorBuildSettings populated yet.
            // Default to the example main scene if present.
            const string defaultScene = "Assets/Scenes/Main.unity";
            if (System.IO.File.Exists(defaultScene))
            {
                scenes = new[] { defaultScene };
            }
        }
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

        BuildPlayerOptions opts = new BuildPlayerOptions();
        opts.scenes = scenes;
        opts.locationPathName = Application.dataPath + "/../" + outputFile;
        opts.options = dev ? BuildOptions.Development : BuildOptions.None;

        return opts;
    }
}
