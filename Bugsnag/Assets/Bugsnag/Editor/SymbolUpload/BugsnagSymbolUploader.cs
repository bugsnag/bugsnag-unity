using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BugsnagUnity.Editor
{

    internal class BugsnagSymbolUploader : IPostprocessBuildWithReport
    {
        public int callbackOrder => 1;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (!IsSupportedPlatform(report.summary.platform))
            {
                return;
            }

            var config = GetSettingsObject();
            if (config == null || !config.AutoUploadSymbols)
            {
                return;
            }

            UnityEngine.Debug.Log($"Starting BugSnag symbol upload for {report.summary.platform}...");

            var buildOutputPath = Path.GetDirectoryName(report.summary.outputPath);

            if (report.summary.platform == BuildTarget.Android)
            {
                EditorUtility.DisplayProgressBar("Bugsnag Symbol Upload", "Uploading Android symbol files", 0.0f);
                if(UploadAndroidSymbols(config.BugsnagCLIExecutablePath, buildOutputPath, config.ApiKey, config.AppVersion, config.VersionCode))
                {
                    UnityEngine.Debug.Log("Android symbol files uploaded to Bugsnag successfully.");
                }
                else
                {
                    UnityEngine.Debug.LogError("Failed to upload Android symbol files to Bugsnag.");
                }
                EditorUtility.ClearProgressBar();
            }
            else if (report.summary.platform == BuildTarget.iOS)
            {
                //TODO implement iOS symbol upload
            }
            else if (report.summary.platform == BuildTarget.StandaloneOSX)
            {
                // TODO implement macOS symbol upload
            }

        }

        private BugsnagSettingsObject GetSettingsObject()
        {
            return Resources.Load<BugsnagSettingsObject>("Bugsnag/BugsnagSettingsObject");
        }

        private bool IsSupportedPlatform(BuildTarget platform)
        {
            return platform == BuildTarget.Android || platform == BuildTarget.iOS || platform == BuildTarget.StandaloneOSX;
        }

        private bool UploadAndroidSymbols(string customExecutablePath, string buildOutputPath, string apiKey, string versionName, int versionCode)
        {

            if (!IsAndroidSymbolCreationEnabled())
            {
                UnityEngine.Debug.LogError("Cannot upload Android symbols to BugSnag because Android symbol creation is disabled in the Unity Build Settings.");
                return false;
            }
            
            return BugsnagCLI.UploadAndroidSymbols(customExecutablePath, buildOutputPath, apiKey, versionName, versionCode);
        }

        private bool IsAndroidSymbolCreationEnabled()
        {
#if UNITY_ANDROID

#if UNITY_2021_1_OR_NEWER
            return EditorUserBuildSettings.androidCreateSymbols == AndroidCreateSymbols.Public ||
                   EditorUserBuildSettings.androidCreateSymbols == AndroidCreateSymbols.Debugging;
#else
            return EditorUserBuildSettings.androidCreateSymbolsZip;
#endif
#endif
            return false;
        }
    }
}