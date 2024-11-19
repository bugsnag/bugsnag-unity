using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BugsnagUnity.Editor
{

    public class BugsnagAndroidSymbolUploader : IPostprocessBuildWithReport
    {
        public int callbackOrder => 1;
        private const string CLI_DOWNLOAD_VERSION = "2.6.2";
        private string _cliDownloadPath = Path.Combine(Application.dataPath, $"../bugsnag/bin/v{CLI_DOWNLOAD_VERSION}/bugsnag_cli");
        private string _cliDownloadUrl = $"https://github.com/bugsnag/bugsnag-cli/releases/download/v{CLI_DOWNLOAD_VERSION}/";

        private BugsnagSettingsObject GetSettingsObject()
        {
            return Resources.Load<BugsnagSettingsObject>("Bugsnag/BugsnagSettingsObject");
        }

        public void OnPostprocessBuild(BuildReport report)
        {

            // Only upload symbols for Android builds
            if (report.summary.platform != BuildTarget.Android)
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

            // Check if symbol creation is enabled
            if (EditorUserBuildSettings.androidCreateSymbols != AndroidCreateSymbols.Public && EditorUserBuildSettings.androidCreateSymbols != AndroidCreateSymbols.Debugging)
            {
                UnityEngine.Debug.LogError("BugSnag symbol upload is enabled but Android symbol creation is disabled. Please enable symbol creation in your build settings.");
                EditorUtility.ClearProgressBar();
                return;
            }

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
                if (!DownloadBugsnagCli(GetBugsnagCliDownloadUrl(), _cliDownloadPath) || !MakeExecutable(_cliDownloadPath))
                {
                    UnityEngine.Debug.LogError("Failed to download and make the Bugsnag CLI executable.");
                    EditorUtility.ClearProgressBar();
                    return; // Exit if the download or chmod process failed
                }
                cliExecutablePath = _cliDownloadPath;
            }


            var buildOutputPath = Path.GetDirectoryName(report.summary.outputPath);

            var args = string.Format("upload unity-android --api-key={0} --verbose --project-root={1} {2}", apiKey, Application.dataPath, buildOutputPath);
            if (!string.IsNullOrEmpty(config.AppVersion))
            {
                args += $" --version-name={config.AppVersion}";
            }
            if (config.VersionCode > -1)
            {
                args += $" --version-code={config.VersionCode}";
            }
            RunBugsnagCliCommand(cliExecutablePath, args);
            EditorUtility.ClearProgressBar();
        }

        private string GetBugsnagCliDownloadUrl()
        {

            // Detect platform
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return RuntimeInformation.OSArchitecture == Architecture.Arm64
                    ? _cliDownloadUrl + "arm64-macos-bugsnag-cli"
                    : _cliDownloadUrl + "x86_64-macos-bugsnag-cli";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return RuntimeInformation.OSArchitecture == Architecture.X86
                    ? _cliDownloadUrl + "i386-windows-bugsnag-cli.exe"
                    : _cliDownloadUrl + "x86_64-windows-bugsnag-cli.exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return RuntimeInformation.OSArchitecture == Architecture.X86
                    ? _cliDownloadUrl + "i386-linux-bugsnag-cli"
                    : _cliDownloadUrl + "x86_64-linux-bugsnag-cli";
            }

            throw new PlatformNotSupportedException("Your platform is not supported for Bugsnag CLI.");
        }

        private bool DownloadBugsnagCli(string cliUrl, string downloadPath)
        {
            UnityEngine.Debug.Log("Starting Bugsnag CLI download process...");

            var directory = Path.GetDirectoryName(downloadPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                UnityEngine.Debug.Log($"Created directory at {directory}");
            }

            if (File.Exists(downloadPath))
            {
                UnityEngine.Debug.Log($"Executable already exists at {downloadPath}, skipping download.");
                return true;
            }

            UnityEngine.Debug.Log($"Downloading Bugsnag CLI from {cliUrl} to {downloadPath}...");

            var downloadProcess = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "curl",
                    Arguments = $"-L {cliUrl} --output {downloadPath}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            downloadProcess.Start();
            string downloadOutput = downloadProcess.StandardOutput.ReadToEnd();
            string downloadError = downloadProcess.StandardError.ReadToEnd();
            downloadProcess.WaitForExit();

            if (downloadProcess.ExitCode != 0)
            {
                UnityEngine.Debug.LogError($"Failed to download the Bugsnag CLI tool. Error: {downloadError}");
                return false;
            }

            UnityEngine.Debug.Log($"Bugsnag CLI tool downloaded successfully. Output: {downloadOutput}");
            return true;
        }

        private bool MakeExecutable(string filePath)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                UnityEngine.Debug.Log($"Making the Bugsnag CLI executable with chmod on Unix/MacOS platform...");

                var chmodProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "chmod",
                        Arguments = $"+x {filePath}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                chmodProcess.Start();
                string chmodOutput = chmodProcess.StandardOutput.ReadToEnd();
                string chmodError = chmodProcess.StandardError.ReadToEnd();
                chmodProcess.WaitForExit();

                if (chmodProcess.ExitCode != 0)
                {
                    UnityEngine.Debug.LogError($"Failed to make the CLI executable. Error: {chmodError}");
                    return false;
                }
                UnityEngine.Debug.Log($"Bugsnag CLI made executable successfully. Output: {chmodOutput}");
            }

            return true;
        }

        private bool RunBugsnagCliCommand(string cliPath, string arguments)
        {
            if (!File.Exists(cliPath))
            {
                UnityEngine.Debug.LogError($"Bugsnag CLI not found at {cliPath}");
                return false;
            }

            UnityEngine.Debug.Log($"Running Bugsnag CLI tool with arguments: {arguments}");

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

            UnityEngine.Debug.Log($"Bugsnag CLI process exited with code {runCliProcess.ExitCode}");

            if (runCliProcess.ExitCode != 0)
            {
                UnityEngine.Debug.LogError($"Failed to run the Bugsnag CLI tool. Error: {runCliError} Output: {runCliOutput}");
                return false;
            }

            UnityEngine.Debug.Log($"Bugsnag CLI tool executed successfully. Output: {runCliOutput}");
            return true;
        }




    }
}