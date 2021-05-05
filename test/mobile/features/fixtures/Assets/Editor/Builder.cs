using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class Builder : MonoBehaviour
{
    static void Build(string folder, BuildTarget target)
    {
        BuildPlayerOptions opts = new BuildPlayerOptions();
        opts.scenes = new[] { "Assets/Scenes/SampleScene.unity", "Assets/Scenes/OtherScene.unity" };
        opts.locationPathName = folder;
        opts.target = target;
        opts.options = BuildOptions.None;

        BuildPipeline.BuildPlayer(opts);
    }

    // Generates iOSBuild/Unity-iPhone.xcodeproj, which can then be used to make an .ipa using
    // xcodebuild
    public static void iOSBuild()
    {
        Debug.Log("Building iOS app...");
        Build("iOSBuild", BuildTarget.iOS);
    }

    // Generates AndroidBuild/MyApp.apk
    public static void AndroidBuild()
    {
        Debug.Log("Building Android app...");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.somthing.somthing");
        Build("AndroidBuild", BuildTarget.Android);
    }
}
#endif
