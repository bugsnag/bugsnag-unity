using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;

public class Builder : MonoBehaviour
{
    // Generates mazerunner.apk
    public static void AndroidBuild()
    {
        Debug.Log("Building Android app...");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.bugsnag.mazerunner");
        var opts = CommonOptions("mazerunner.apk");
        opts.target = BuildTarget.Android;

        var result = BuildPipeline.BuildPlayer(opts);

        Debug.Log("Result: " + result);
    }

    // Generates mazerunner.apk
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

    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        Debug.Log("SKW:" + pathToBuiltProject);
    }
}
#endif
