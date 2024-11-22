using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BugsnagUnity.Editor
{
    internal class BugsnagCLI
    {
        private const string DOWNLOADED_CLI_VERSION = "2.6.2";
        private readonly string DOWNLOADED_CLI_PATH = Path.Combine(Application.dataPath, "../bugsnag/bin/bugsnag_cli");
        private readonly string DOWNLOADED_CLI_URL = $"https://github.com/bugsnag/bugsnag-cli/releases/download/v{DOWNLOADED_CLI_VERSION}/";
        private readonly string _cliExecutablePath;
        public BugsnagCLI()
        {
            try
            {
                var config = BugsnagSettingsObject.GetSettingsObject();
                if (config == null)
                {
                    return;
                }
                if (string.IsNullOrEmpty(config.BugsnagCLIExecutablePath))
                {
                    _cliExecutablePath = GetBugsnagCLI();
                }
                else
                {
                    _cliExecutablePath = config.BugsnagCLIExecutablePath;
                }
                if (!File.Exists(_cliExecutablePath))
                {
                    throw new Exception($"BugSnag CLI not found at expected path: {_cliExecutablePath}");
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to setup Bugsnag CLI: {ex.Message}");
            }
        }

        public void UploadAndroidSymbols(string buildOutputPath, string apiKey, string versionName, int versionCode)
        {
            string args = $"upload unity-android --api-key={apiKey} --verbose --project-root={Application.dataPath} {buildOutputPath}";

            if (!string.IsNullOrEmpty(versionName))
                args += $" --version-name={versionName}";
            if (versionCode > -1)
                args += $" --version-code={versionCode}";

            int exitCode = StartProcess(_cliExecutablePath, args, out string output, out string error);

            if (exitCode != 0)
            {
                throw new Exception($"Error uploading symbols: {error}\nOutput: {output}");
            }
        }

        public string GetBugsnagCLI()
        {
            try
            {
                EnsureDirectoryExists();
                if (File.Exists(DOWNLOADED_CLI_PATH) && GetCurrentCliVersion() == DOWNLOADED_CLI_VERSION)
                {
                    return DOWNLOADED_CLI_PATH;
                }
                DownloadCLI();
                MakeExecutable();
                return DOWNLOADED_CLI_PATH;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to setup Bugsnag CLI: {ex.Message}");
                return null;
            }
        }

        private void EnsureDirectoryExists()
        {
            var directory = Path.GetDirectoryName(DOWNLOADED_CLI_PATH);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private void DownloadCLI()
        {
            string url = GetDownloadUrl();
            if (string.IsNullOrEmpty(url))
            {
                throw new InvalidOperationException($"Unsupported platform: {RuntimeInformation.OSDescription}");
            }

            string tempPath = DOWNLOADED_CLI_PATH + ".tmp";

            try
            {
                int exitCode = StartProcess("curl", $"-L {url} --output {tempPath}", out string output, out string error);
                if (exitCode != 0)
                {
                    throw new InvalidOperationException($"Download failed: {error}");
                }

                if (File.Exists(DOWNLOADED_CLI_PATH))
                {
                    File.Delete(DOWNLOADED_CLI_PATH);
                }

                File.Move(tempPath, DOWNLOADED_CLI_PATH);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        private string GetDownloadUrl()
        {
            string fileName = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                fileName = RuntimeInformation.OSArchitecture == Architecture.Arm64
                    ? "arm64-macos-bugsnag-cli"
                    : "x86_64-macos-bugsnag-cli";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                fileName = RuntimeInformation.OSArchitecture == Architecture.X86
                    ? "i386-windows-bugsnag-cli.exe"
                    : "x86_64-windows-bugsnag-cli.exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                fileName = RuntimeInformation.OSArchitecture == Architecture.X86
                    ? "i386-linux-bugsnag-cli"
                    : "x86_64-linux-bugsnag-cli";
            }

            return fileName != null ? DOWNLOADED_CLI_URL + fileName : null;
        }

        private void MakeExecutable()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                int exitCode = StartProcess("chmod", $"+x {DOWNLOADED_CLI_PATH}", out _, out _);
                if(exitCode != 0)
                {
                    throw new InvalidOperationException("Failed to make CLI executable");
                }
            }
        }

        private string GetCurrentCliVersion()
        {
            if (!File.Exists(DOWNLOADED_CLI_PATH))
            {
                UnityEngine.Debug.LogError($"Bugsnag CLI not found at {DOWNLOADED_CLI_PATH}");
                return null;
            }

            int exitCode = StartProcess(_cliExecutablePath, "--version", out string output, out string error);
            if (exitCode != 0)
            {
                UnityEngine.Debug.LogError($"Error checking CLI version: {error}");
                return null;
            }

            return output.Trim();
        }

        private int StartProcess(string fileName, string arguments, out string standardOutput, out string standardError)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            standardOutput = process.StandardOutput.ReadToEnd();
            standardError = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return process.ExitCode;
        }

    }
}