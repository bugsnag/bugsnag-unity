
// This script fixes an issue in versions 1.7.0 and below of the BugSnag Performance package where the
// BugsnagPerformance.asmdef file was not correctly configured to reference the BugsnagUnity package
// (and therefore cannot access the version of the BugsnagUnityWebRequest class packaged inside it).
// This script adds the reference to the BugSnag Performance package assembly definition, if not 
// present, for backwards compatibility with older versions (<=1.7.0).

using System.IO;
using UnityEditor;
using UnityEngine;

namespace BugsnagUnity.Editor
{
    [InitializeOnLoad]
    public class BugsnagEditPerformanceAssemblyDefinition
    {
        static BugsnagEditPerformanceAssemblyDefinition()
        {
            try
            {
                FixBugsnagPerformanceAsmdef();
            }
            catch
            {
                // ignore any unexpected permissions or path errors.
            }
        }

        public static void FixBugsnagPerformanceAsmdef()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string packageCachePath = Path.Combine(projectRoot, "Library", "PackageCache");

            if (!Directory.Exists(packageCachePath))
            {
                // Package cache not found, nothing to do
                return;
            }

            string[] matchingDirs = Directory.GetDirectories(packageCachePath, "com.bugsnag.performance.unity*");
            if (matchingDirs.Length == 0)
            {
                // Bugsnag Performance package not found, nothing to do
                return;
            }

            string bugsnagPackageDir = matchingDirs[0];
            string asmdefPath = Path.Combine(bugsnagPackageDir, "Runtime", "Scripts", "BugsnagPerformance.asmdef");

            if (!File.Exists(asmdefPath))
            {
                // BugsnagPerformance.asmdef not found, nothing to do
                return;
            }

            string originalJson = File.ReadAllText(asmdefPath).Trim();
            string strippedJson = originalJson.Replace(" ", "").Replace("\n", "").Replace("\r", "");

            // Check if the "references":["BugsnagUnity"] entry already exists
            if (strippedJson.Contains("\"references\":[\"BugsnagUnity\"]"))
            {
                // BugsnagUnity reference already present, nothing to do
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
            Debug.Log("Updated BugsnagPerformance.asmdef with a reference to BugsnagUnity. If you continue to see errors related to the BugsnagUnityWebRequest class after restarting Unity, please update your BugsnagPerformance package or add a reference in the .asmdef file manually.");
            AssetDatabase.Refresh();
        }
    }
}