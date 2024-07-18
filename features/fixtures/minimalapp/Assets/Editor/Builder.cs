using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;

public class Builder : MonoBehaviour
{


    public static void BuildAndroidWith()
    {
        AndroidBuild("_with_bugsnag");
    }

    public static void BuildAndroidWithout()
    {
        AndroidBuild("_without_bugsnag");
    }

    // Generates the Mazerunner APK
    private static void AndroidBuild(string type)
    {
        var opts = CommonOptions("minimal" + type + ".apk");
        opts.target = BuildTarget.Android;
        BuildPipeline.BuildPlayer(opts);
    }

    public static void BuildIosWithout()
    {
        IosBuild("without");
    }

     public static void BuildIosWith()
    {
        IosBuild("with");
    }
      
    private static void IosBuild(string type)
    {
        PlayerSettings.iOS.appleDeveloperTeamID = "7W9PZ27Y5F";
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        PlayerSettings.iOS.allowHTTPDownload = true;
        var opts = CommonOptions("minimal_" + type + "_xcode");
        opts.target = BuildTarget.iOS;
        var result = BuildPipeline.BuildPlayer(opts);
    }

    private static BuildPlayerOptions CommonOptions(string outputFile)
    {
        BuildPlayerOptions opts = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = Application.dataPath + "/../" + outputFile,
            options = BuildOptions.None
        };
        return opts;
    }
}
#endif
