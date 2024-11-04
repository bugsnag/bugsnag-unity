using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ForceImportSettings : MonoBehaviour
{
    // Static constructor is called automatically when the project is opened
    static ForceImportSettings()
    {
        ApplyPluginImportSettings("Assets/Bugsnag/Plugins/iOS", new List<BuildTarget> { BuildTarget.iOS });
        ApplyPluginImportSettings("Assets/Bugsnag/Plugins/MacOS", new List<BuildTarget> { BuildTarget.StandaloneOSX });
        ApplyPluginImportSettings("Assets/Bugsnag/Plugins/tvOS", new List<BuildTarget> { BuildTarget.tvOS });
        ApplyPluginImportSettings("Assets/Bugsnag/Plugins/Android", new List<BuildTarget> { BuildTarget.Android });
        ApplyPluginImportSettings("Assets/Bugsnag/Plugins/Cocoa", new List<BuildTarget> { BuildTarget.iOS, BuildTarget.tvOS, BuildTarget.StandaloneOSX });
    }

    private static List<BuildTarget> RelevantBuildTargets = new List<BuildTarget> {
        BuildTarget.iOS,
        BuildTarget.tvOS,
        BuildTarget.Android,
        BuildTarget.StandaloneOSX,
        BuildTarget.StandaloneWindows,
        BuildTarget.StandaloneWindows64,
        BuildTarget.WebGL,
        BuildTarget.Switch
    };

    private static void ApplyPluginImportSettings(string dir, List<BuildTarget> targets)
    {
        string[] files = System.IO.Directory.GetFiles(dir, "*", System.IO.SearchOption.AllDirectories);

        foreach (string file in files)
        {
            // Get the asset importer for each file
            string assetPath = file.Replace("\\", "/"); // Ensure compatibility on Windows
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);

            if (importer != null && !assetPath.EndsWith(".meta"))
            {
                if (importer is PluginImporter pluginImporter)
                {
                    foreach (var target in RelevantBuildTargets)
                    {
                        pluginImporter.SetCompatibleWithPlatform(target, targets.Contains(target));
                    }
                    pluginImporter.SaveAndReimport();
                }

                Debug.Log($"Set import settings for {assetPath}");
            }
        }

        Debug.Log("import settings applied.");
    }
}