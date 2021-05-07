using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class Builder : MonoBehaviour
{
    // Generates AndroidBuild/MyApp.apk
    public static void AndroidBuild()
    {
        Debug.Log("Building Android app...");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.bugsnag.mazerunner");
        var opts = CommonOptions("mazerunner.apk");
        opts.target = BuildTarget.Android;

        var result = BuildPipeline.BuildPlayer(opts);

        Debug.Log("Result: " + result);
    }

    private static BuildPlayerOptions CommonOptions(string outputFile)
    {
        BuildPlayerOptions opts = new BuildPlayerOptions();
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        opts.scenes = scenes;
        opts.locationPathName = Application.dataPath + "/../" + outputFile;
        opts.options = BuildOptions.None;

        return opts;
    }
}
#endif
