using System.IO;
using UnityEditor;
using UnityEngine;
namespace BugsnagUnity.Editor
{
[InitializeOnLoad]
public class BugsnagEditPerformanceAssemblyDeff
{

    // This script fixes an issue in versions 1.7.0 and bellow of the Bugsnag Performance package.
    // The issue is that the BugsnagPerformance.asmdef file is not correctly configured to reference the BugsnagUnity package.
    // And therefore cannot access the version of the BugsnagUnityWebRequest class packaged with the notifier.
    // A fix for this issue was released in the 1.7.1 version of the Bugsnag Performance package. 
    // So this fix is only to support using legacy versions of the perf sdk.

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