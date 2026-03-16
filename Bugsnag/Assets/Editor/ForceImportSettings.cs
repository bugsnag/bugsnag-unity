using System.Collections.Generic;
using System.IO;
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
        // There is a bug in some unity versions where MacOS bundle plugins are imported as a directory instead of a single plugin file.
        // This causes issues for UPM installs, because if the plugin is imported as a directory, then the package manager will expect
        // each file to have a .meta file with it and throws an error when non is found.
        // This method is a work around that manually adds those meta files.
        GenerateMetaFilesForBundle();
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
                    }
                }
            }
        }
        AssetDatabase.Refresh();
    }

    private static void GenerateMetaFilesForBundle()
    {
        string sourceDir = "Assets/Bugsnag/Plugins/MacOS/bugsnag-osx.bundle";
        string tempDir = "Assets/Bugsnag/Plugins/MacOS/bundleMetaGeneration";
        string tempDirMeta = "Assets/Bugsnag/Plugins/MacOS/bundleMetaGeneration.meta";

        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, true);
        }
        Directory.CreateDirectory(tempDir);
        CopyDirectory(sourceDir, tempDir);

        AssetDatabase.ImportAsset(tempDir, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();
        Directory.Delete(sourceDir, true);
        Directory.CreateDirectory(sourceDir);
        CopyDirectory(tempDir, sourceDir);
        Directory.Delete(tempDir, true);
        File.Delete(tempDirMeta);
        AssetDatabase.Refresh();
    }

    private static void CopyDirectory(string sourceDir, string targetDir)
    {
        foreach (var dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourceDir, targetDir));
        }

        foreach (var filePath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(filePath, filePath.Replace(sourceDir, targetDir), true);
        }
    }
}