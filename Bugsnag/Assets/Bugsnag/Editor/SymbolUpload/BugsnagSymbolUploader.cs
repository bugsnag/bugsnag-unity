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

            string cliPath = GetBugsnagCLIPath(config);
            if (string.IsNullOrEmpty(cliPath))
            {
                UnityEngine.Debug.LogError("Bugsnag CLI is not available. Symbol upload aborted.");
                return;
            }

            var buildOutputPath = Path.GetDirectoryName(report.summary.outputPath);

            if (report.summary.platform == BuildTarget.Android)
            {
                UploadAndroidSymbols(buildOutputPath, cliPath, config);
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

        private string GetBugsnagCLIPath(BugsnagSettingsObject config)
        {
            if (!string.IsNullOrEmpty(config.BugsnagCLIExecutablePath))
            {
                return config.BugsnagCLIExecutablePath;
            }

            return BugsnagCLIDownloader.DownloadBugsnagCli();
        }

        private void UploadAndroidSymbols(string buildPath, string cliPath, BugsnagSettingsObject config)
        {
            EditorUtility.DisplayProgressBar("Bugsnag Symbol Upload", "Uploading Android symbol files", 0.0f);

            if (!IsAndroidSymbolsEnabled())
            {
                UnityEngine.Debug.LogError("Android symbol creation is disabled. Enable it in the build settings.");
                EditorUtility.ClearProgressBar();
                return;
            }

            string args = BuildAndroidCLIArgs(buildPath, config);
            if (RunBugsnagCliCommand(cliPath, args))
            {
                UnityEngine.Debug.Log("Android symbol files uploaded to Bugsnag successfully.");
            }
            EditorUtility.ClearProgressBar();
        }

        private bool IsAndroidSymbolsEnabled()
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

        private string BuildAndroidCLIArgs(string buildPath, BugsnagSettingsObject config)
        {
            string args = $"upload unity-android --api-key={config.ApiKey} --verbose --project-root={Application.dataPath} {buildPath}";

            if (!string.IsNullOrEmpty(config.AppVersion))
                args += $" --version-name={config.AppVersion}";
            if (config.VersionCode > -1)
                args += $" --version-code={config.VersionCode}";

            return args;
        }

        private bool RunBugsnagCliCommand(string cliPath, string arguments)
        {
            if (!File.Exists(cliPath))
            {
                UnityEngine.Debug.LogError($"Bugsnag CLI not found at {cliPath}");
                return false;
            }

            UnityEngine.Debug.Log($"Executing Bugsnag CLI:\n{cliPath} {arguments}");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = cliPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                UnityEngine.Debug.LogError($"Error uploading symbols: {error}\nOutput: {output}");
                return false;
            }

            return true;
        }
    }
}