using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ForceImportSettings : MonoBehaviour
{
    public static void ApplyImportSettings()
    {
        ApplyPluginImportSettings("Assets/Bugsnag/Plugins/iOS", new List<BuildTarget> { BuildTarget.iOS });
        ApplyPluginImportSettings("Assets/Bugsnag/Plugins/MacOS", new List<BuildTarget> { BuildTarget.StandaloneOSX });
        ApplyPluginImportSettings("Assets/Bugsnag/Plugins/tvOS", new List<BuildTarget> { BuildTarget.tvOS });
        ApplyPluginImportSettings("Assets/Bugsnag/Plugins/Android", new List<BuildTarget> { BuildTarget.Android });
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
        string[] guids = AssetDatabase.FindAssets("", new[] { dir });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            if (!string.IsNullOrEmpty(assetPath) && !assetPath.EndsWith(".meta"))
            {
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);

                if (importer != null)
                {
                    if (importer is PluginImporter pluginImporter)
                    {
                        pluginImporter.SetCompatibleWithAnyPlatform(false);
                        foreach (var target in RelevantBuildTargets)
                        {
                            pluginImporter.SetCompatibleWithPlatform(target, targets.Contains(target));
                        }
                        pluginImporter.SaveAndReimport();

                        Debug.Log($"Set import settings for {assetPath}");
                    }
                }
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Import settings applied and Asset Database refreshed.");
    }
}