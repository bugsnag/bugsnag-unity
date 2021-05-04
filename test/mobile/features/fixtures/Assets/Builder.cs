using UnityEngine;

#if UNITY_EDITOR // all of the build tools-related stuff does not exist at runtime in the app
using UnityEditor;

class Sample : MonoBehaviour
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
    static void iOSBuild()
    {
        Debug.Log("Building iOS app...");
        Build("iOSBuild", BuildTarget.iOS);
    }

    // Generates AndroidBuild/MyApp.apk
    static void AndroidBuild()
    {
        Debug.Log("Building Android app...");
        Build("AndroidBuild", BuildTarget.Android);
    }
}
#endif