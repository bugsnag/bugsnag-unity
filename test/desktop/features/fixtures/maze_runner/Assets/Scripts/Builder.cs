using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
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

    // Generates Mazerunner.app
    public static void MacOS()
    {
        Build("Mazerunner", BuildTarget.StandaloneOSX);
    }

    // Generates Mazerunner.app
    public static void Win64()
    {
        Build("WindowsBuild/Mazerunner.exe", BuildTarget.StandaloneWindows64);
    }
}
#endif
