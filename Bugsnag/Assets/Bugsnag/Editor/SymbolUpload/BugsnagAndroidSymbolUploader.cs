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
                if (!DownloadBugsnagCli(GetBugsnagCliDownloadUrl(), _cliDownloadPath))
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
            if (RunBugsnagCliCommand(cliExecutablePath, args))
            {
                UnityEngine.Debug.Log("Symbol files uploaded successfully");
            }
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

            UnityEngine.Debug.LogError($"Unable to run the BugSnag CLI on {RuntimeInformation.OSDescription} platform.");

            return null;
        }

        private bool DownloadBugsnagCli(string cliUrl, string downloadPath)
        {
            var directory = Path.GetDirectoryName(downloadPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Remove any older CLI downloads
            var parentDirectory = Directory.GetParent(directory)?.FullName;
            if (parentDirectory != null)
            {
                var directoryName = Path.GetFileName(directory);
                foreach (var subdirectory in Directory.GetDirectories(parentDirectory))
                {
                    var subdirectoryName = Path.GetFileName(subdirectory);
                    if (!string.Equals(subdirectoryName, directoryName, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            Directory.Delete(subdirectory, true);
                        }
                        catch { }
                    }
                }
            }

            if (File.Exists(downloadPath))
            {
                return true;
            }
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
            string downloadError = downloadProcess.StandardError.ReadToEnd();
            downloadProcess.WaitForExit();

            if (downloadProcess.ExitCode != 0)
            {
                UnityEngine.Debug.LogError($"Failed to download the BugSnag CLI. Error: {downloadError}");
                return false;
            }
            return MakeCLIExecutable(_cliDownloadPath);
        }

        private bool MakeCLIExecutable(string filePath)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
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
                    return false;
                }
            }

            return true;
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