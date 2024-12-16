using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BugsnagUnity.Editor
{
    internal class BugsnagCLI
    {
        private const string DOWNLOADED_CLI_VERSION = "2.7.0";
        private readonly string DOWNLOADED_CLI_PATH = Path.Combine(Application.dataPath, "../bugsnag/bin/bugsnag_cli");
        private readonly string DOWNLOADED_CLI_URL = $"https://github.com/bugsnag/bugsnag-cli/releases/download/v{DOWNLOADED_CLI_VERSION}/";
        private readonly string _cliExecutablePath;
        public BugsnagCLI()
        {
            var config = BugsnagSettingsObject.GetSettingsObject();

            if (string.IsNullOrEmpty(config.BugsnagCLIExecutablePath))
            {
                _cliExecutablePath = DownloadBugsnagCLI();
            }
            else
            {
                _cliExecutablePath = config.BugsnagCLIExecutablePath;
            }
            if (!File.Exists(_cliExecutablePath))
            {
                throw new Exception($"BugSnag CLI not found at path: {_cliExecutablePath}");
            }
        }

        public void UploadAndroidSymbols(string buildOutputPath, string apiKey, string versionName, int versionCode, string uploadEndpoint)
        {
            string args = $"upload unity-android --api-key={apiKey} --verbose --project-root={Application.dataPath} {buildOutputPath}";

            if (!string.IsNullOrEmpty(versionName))
            {
                args += $" --version-name={versionName}";
            }
            if (versionCode > -1)
            {
                args += $" --version-code={versionCode}";
            }
            if (!string.IsNullOrEmpty(uploadEndpoint))
            {
                args += $" --upload-api-root-url={uploadEndpoint}";
            }
            int exitCode = StartProcess(_cliExecutablePath, args, out string output, out string error);

            if (exitCode != 0)
            {
                throw new Exception($"Error uploading symbols: {error}\nOutput: {output}");
            }
        }

        public string DownloadBugsnagCLI()
        {
            EnsureDirectoryExists();
            if (File.Exists(DOWNLOADED_CLI_PATH) && GetCurrentCliVersion() == DOWNLOADED_CLI_VERSION)
            {
                return DOWNLOADED_CLI_PATH;
            }
            DownloadCLI();
            MakeFileExecutable(DOWNLOADED_CLI_PATH);
            return DOWNLOADED_CLI_PATH;
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

        private void MakeFileExecutable(string path)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                int exitCode = StartProcess("chmod", $"+x {path}", out _, out _);
                if (exitCode != 0)
                {
                    throw new InvalidOperationException($"Failed to make file at {path} executable");
                }
            }
        }

        private string GetCurrentCliVersion()
        {
            if (!File.Exists(DOWNLOADED_CLI_PATH))
            {
                UnityEngine.Debug.LogError($"BugSnag CLI not found at {DOWNLOADED_CLI_PATH}");
                return null;
            }

            int exitCode = StartProcess(DOWNLOADED_CLI_PATH, "--version", out string output, out string error);
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

        public string GetIosDsymUploadCommand(string apiKey, string uploadEndpoint, string workspacePath)
        {
            var command = $"{_cliExecutablePath} upload xcode-archive --api-key={apiKey} --scheme=Unity-iPhone {workspacePath}";
            if (!string.IsNullOrEmpty(uploadEndpoint))
            {
                command += $" --upload-api-root-url={uploadEndpoint}";
            }
            return command;
        }

    }
}

