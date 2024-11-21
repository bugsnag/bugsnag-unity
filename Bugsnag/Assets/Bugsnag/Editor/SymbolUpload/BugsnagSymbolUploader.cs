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


        private BugsnagSettingsObject GetSettingsObject()
        {
            return Resources.Load<BugsnagSettingsObject>("Bugsnag/BugsnagSettingsObject");
        }

        public void OnPostprocessBuild(BuildReport report)
        {

            // Only upload symbols for Supported Platforms builds
            if (report.summary.platform != BuildTarget.Android &&
                report.summary.platform != BuildTarget.iOS &&
                report.summary.platform != BuildTarget.StandaloneOSX)
            {
                return;
            }

            // Get the Bugsnag settings object and check if auto-upload is enabled
            var config = GetSettingsObject();
            if (config == null || !config.AutoUploadSymbols)
            {
                return;
            }

            UnityEngine.Debug.Log("Starting BugSnag Android symbol upload.");
            EditorUtility.DisplayProgressBar("BugSnag Symbol Upload", "Uploading Android symbol files", 0.0f);



            // check if the API key is set
            var apiKey = config.ApiKey;
            if (string.IsNullOrEmpty(apiKey))
            {
                UnityEngine.Debug.LogError("BugSnag symbol upload is enabled but your BugSnag API key is not set. Please set the API key in your BugSnag configuration.");
                EditorUtility.ClearProgressBar();
                return;
            }

            // Check if a cli path was provided in the settings object, if yes use it if not download the cli
            string cliExecutablePath;
            if (!string.IsNullOrEmpty(config.BugsnagCLIExecutablePath))
            {
                cliExecutablePath = config.BugsnagCLIExecutablePath;
            }
            else
            {
                cliExecutablePath = BugsnagCLIDownloader.DownloadBugsnagCli();
            }

            if (string.IsNullOrEmpty(cliExecutablePath))
            {
                UnityEngine.Debug.LogError("Failed to download and make the Bugsnag CLI executable.");
                EditorUtility.ClearProgressBar();
                return;
            }

            var buildOutputPath = Path.GetDirectoryName(report.summary.outputPath);

            if (report.summary.platform == BuildTarget.Android)
            {
                UploadAndroidSymbols(buildOutputPath, cliExecutablePath, config);
            }
            else if (report.summary.platform == BuildTarget.iOS)
            {
                //TODO Append xcode project with post build script
            }
            else if (report.summary.platform == BuildTarget.StandaloneOSX)
            {
                //TODO Check for xcode build setting and append xcode build
            }
           
            EditorUtility.ClearProgressBar();
        }

        private void UploadAndroidSymbols(string buildPath, string cliPath, BugsnagSettingsObject config)
        {
            // Check if symbol creation is enabled
            if (EditorUserBuildSettings.androidCreateSymbols != AndroidCreateSymbols.Public && EditorUserBuildSettings.androidCreateSymbols != AndroidCreateSymbols.Debugging)
            {
                UnityEngine.Debug.LogError("BugSnag symbol upload is enabled but Android symbol creation is disabled. Please enable symbol creation in your build settings.");
                EditorUtility.ClearProgressBar();
                return;
            }

             var args = string.Format("upload unity-android --api-key={0} --verbose --project-root={1} {2}", config.ApiKey, Application.dataPath, buildPath);
            if (!string.IsNullOrEmpty(config.AppVersion))
            {
                args += $" --version-name={config.AppVersion}";
            }
            if (config.VersionCode > -1)
            {
                args += $" --version-code={config.VersionCode}";
            }
            if (RunBugsnagCliCommand(cliPath, args))
            {
                UnityEngine.Debug.Log("Symbol files uploaded to BugSnag successfully");
            }
        }


        private bool RunBugsnagCliCommand(string cliPath, string arguments)
        {
            if (!File.Exists(cliPath))
            {
                UnityEngine.Debug.LogError($"BugSnag CLI not found at {cliPath}");
                return false;
            }

            UnityEngine.Debug.Log($"Running BugSnag CLI:\n{cliPath} {arguments}");

            var runCliProcess = new Process()
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

            runCliProcess.Start();
            string runCliOutput = runCliProcess.StandardOutput.ReadToEnd();
            string runCliError = runCliProcess.StandardError.ReadToEnd();
            runCliProcess.WaitForExit();

            if (runCliProcess.ExitCode != 0)
            {
                UnityEngine.Debug.LogError($"An error occurred uploading symbol files: {runCliError}.\nCLI output: {runCliOutput}");
                return false;
            }

            return true;
        }
    }
}