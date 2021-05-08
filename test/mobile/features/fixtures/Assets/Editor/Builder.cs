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
        var paths = new string[] { "Assets/Scenes/SampleScene.unity", "Assets/Scenes/OtherScene.unity" };
        var newScenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene()
            {
                path = paths[0],
                enabled = true
            },
            new EditorBuildSettingsScene()
            {
                path = paths[1],
                enabled = true
            }
        };

        EditorBuildSettings.scenes = newScenes;

        BuildPlayerOptions opts = new BuildPlayerOptions();
        opts.scenes = paths;
        opts.locationPathName = Application.dataPath + "/../" + outputFile;
        opts.options = BuildOptions.None;

        return opts;
    }
}
#endif
