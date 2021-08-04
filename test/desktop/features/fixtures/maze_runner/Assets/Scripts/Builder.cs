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
    public static void MacOSBuild()
    {
        Build("Mazerunner", BuildTarget.StandaloneOSX);
    }
}
#endif
