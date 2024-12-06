using System.IO;
using UnityEditor;
using UnityEngine;
namespace BugsnagUnity.Editor
{
[InitializeOnLoad]
public class BugsnagEditPerformanceAssemblyDeff
{

    // This script fixes an issue in versions 1.7.0 and below of the BugSnag Performance package where the
    // BugsnagPerformance.asmdef file was not correctly configured to reference the BugsnagUnity package
    // (and therefore cannot access the version of the BugsnagUnityWebRequest class packaged inside it).
    // This script adds the reference to the BugSnag Performance package assembly definition, if not 
    // present, for backwards compatibility with older versions (<=1.7.0).

    static BugsnagEditPerformanceAssemblyDeff()
    {
        FixBugsnagPerformanceAsmdef();
    }
    public static void FixBugsnagPerformanceAsmdef()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string packageCachePath = Path.Combine(projectRoot, "Library", "PackageCache");
        if (!Directory.Exists(packageCachePath))
        {
            // Package cache directory doesn't exist, so we can't fix the asmdef
            return;
        }
        string[] matchingDirs = Directory.GetDirectories(packageCachePath, "com.bugsnag.performance.unity*");
        if (matchingDirs.Length == 0)
        {
            // No Bugsnag Performance package found
            return;
        }

        string bugsnagPackageDir = matchingDirs[0];
        string asmdefPath = Path.Combine(bugsnagPackageDir, "Runtime", "Scripts", "BugsnagPerformance.asmdef");
        if (!File.Exists(asmdefPath))
        {
            return;
        }
        string originalJson = File.ReadAllText(asmdefPath).Trim();
        string minimalJson = "{\n\t\"name\": \"BugsnagPerformance\"\n}";
        string minimalJsonNoWhitespace = "{\"name\":\"BugsnagPerformance\"}";
        bool isMinimal = originalJson == minimalJson || originalJson == minimalJsonNoWhitespace;
        if (!isMinimal)
        {
            // The asmdef is already expanded, so we don't need to do anything
            return;
        }

        string expandedJson = @"{
    ""name"": ""BugsnagPerformance"",
    ""rootNamespace"": """",
    ""references"": [
        ""BugsnagUnity""
    ],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}";
        File.WriteAllText(asmdefPath, expandedJson);
        AssetDatabase.Refresh();
    }
}
}