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
        opts.options = BuildOptions.None;
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

    // Generates the Mazerunner APK
    public static void AndroidBuild()
    {
        Debug.Log("Building Android app...");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.bugsnag.mazerunner");
        var opts = CommonOptions("mazerunner.apk");
        opts.target = BuildTarget.Android;

        var result = BuildPipeline.BuildPlayer(opts);
        Debug.Log("Result: " + result);
    }

    // Generates the Mazerunner IPA
    public static void IosBuild()
    {
        Debug.Log("Building iOS app...");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.bugsnag.unity.mazerunner");
        PlayerSettings.iOS.appleDeveloperTeamID = "372ZUL2ZB7";
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        PlayerSettings.iOS.allowHTTPDownload = true;

        var opts = CommonOptions("mazerunner_xcode");
        opts.target = BuildTarget.iOS;

        var result = BuildPipeline.BuildPlayer(opts);
        Debug.Log("Result: " + result);
    }

    public static void SwitchBuild()
    {
        Debug.Log("Building Switch app...");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Switch, "com.bugsnag.mazerunner");
        var opts = CommonOptions("mazerunner.nspd");
        opts.target = BuildTarget.Switch;
        opts.options = BuildOptions.Development;

        var result = BuildPipeline.BuildPlayer(opts);
        Debug.Log("Result: " + result);
    }

    private static BuildPlayerOptions CommonOptions(string outputFile)
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        BuildPlayerOptions opts = new BuildPlayerOptions();
        opts.scenes = scenes;
        opts.locationPathName = Application.dataPath + "/../" + outputFile;
        opts.options = BuildOptions.None;

        return opts;
    }
}
#endif
