using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BugsnagUnity.Editor;
using UnityEditor;
using UnityEngine;

public class Builder : MonoBehaviour
{
    public static void AndroidBuild()
    {
        Debug.Log("Building Android app...");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.bugsnag.edm");
        var opts = CommonOptions("edm.apk");
        opts.target = BuildTarget.Android;

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

    public static void EnableEDM()
    {
        BugsnagEditor.EnableEDM();
    }
}
