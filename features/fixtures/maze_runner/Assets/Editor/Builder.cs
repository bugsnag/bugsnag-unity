using System.Linq;
using UnityEngine;
using BugsnagUnity.Editor;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;

public class Builder : MonoBehaviour {

    static void Build(string folder, BuildTarget target)
    {
        BuildPlayerOptions opts = new BuildPlayerOptions();
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        opts.scenes = scenes;
        opts.locationPathName = folder;
        opts.target = target;
        opts.options = BuildOptions.Development;
        BuildPipeline.BuildPlayer(opts);
    }

    public static void MacOS()
    {
        Build("build/MacOS/Mazerunner", BuildTarget.StandaloneOSX);
    }

    public static void Win64()
    {
        Build("build/Windows/Mazerunner.exe", BuildTarget.StandaloneWindows64);
    }

    public static void WebGL()
    {
        Build("build/WebGL/Mazerunner", BuildTarget.WebGL);
    }


    public static void AndroidDev()
    {
        BuildAndroid(true);
    }

    public static void AndroidRelease()
    {
        BuildAndroid(false);
    }
    // Generates the Mazerunner APK
    static void BuildAndroid(bool dev)
    {
        Debug.Log("Building Android app...");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.bugsnag.fixtures.unity.notifier.android");
        var opts = CommonMobileBuildOptions(dev ? "mazerunner_dev.apk" : "mazerunner.apk", dev);
        opts.target = BuildTarget.Android;

#if UNITY_2022_1_OR_NEWER
        PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;
#endif

        var result = BuildPipeline.BuildPlayer(opts);
        Debug.Log("Result: " + result);
    }



    // Generates the Mazerunner IPA
    public static void IosDev()
    {
        IosBuild(true);
    }

    public static void IosRelease()
    {
        IosBuild(false);
    }
    static void IosBuild(bool dev)
    {
        Debug.Log("Building iOS app...");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.bugsnag.fixtures.unity.notifier.ios");
        PlayerSettings.iOS.appleDeveloperTeamID = "7W9PZ27Y5F";
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        PlayerSettings.iOS.allowHTTPDownload = true;
        var opts = CommonMobileBuildOptions(dev ? "mazerunner_dev_xcode" : "mazerunner_xcode", dev);
        opts.target = BuildTarget.iOS;
        var result = BuildPipeline.BuildPlayer(opts);
        Debug.Log("Result: " + result);
    }

    public static void SwitchBuild()
    {
        Debug.Log("Building Switch app...");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Switch, "com.bugsnag.fixtures.unity.notifier.ios");
        var opts = CommonMobileBuildOptions("mazerunner.nspd",false);
        opts.target = BuildTarget.Switch;
        opts.options = BuildOptions.Development;

        var result = BuildPipeline.BuildPlayer(opts);
        Debug.Log("Result: " + result);
    }

    private static BuildPlayerOptions CommonMobileBuildOptions(string outputFile, bool dev)
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        BuildPlayerOptions opts = new BuildPlayerOptions();
        opts.scenes = scenes;
        opts.locationPathName = Application.dataPath + "/../" + outputFile;
        opts.options = dev ? BuildOptions.Development : BuildOptions.None;

        return opts;
    }
}
#endif
