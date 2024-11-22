using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BugsnagUnity.Editor
{
    internal class BugsnagCLI
    {
        private const string CLI_VERSION = "2.6.2";
        private static readonly string CLI_PATH = Path.Combine(Application.dataPath, "../bugsnag/bin/bugsnag_cli");
        private static readonly string BASE_DOWNLOAD_URL = $"https://github.com/bugsnag/bugsnag-cli/releases/download/v{CLI_VERSION}/";

        public static bool UploadAndroidSymbols(string customExecutablePath, string buildOutputPath, string apiKey, string versionName, int versionCode)
        {
            string args = $"upload unity-android --api-key={apiKey} --verbose --project-root={Application.dataPath} {buildOutputPath}";

            if (!string.IsNullOrEmpty(versionName))
                args += $" --version-name={versionName}";
            if (versionCode > -1)
                args += $" --version-code={versionCode}";

            return RunCLICommand(customExecutablePath, args);
        }

        private static bool RunCLICommand(string customExecutablePath, string args)
        {
            string executablePath = File.Exists(customExecutablePath) ? customExecutablePath : GetBugsnagCLI();

            if (string.IsNullOrEmpty(executablePath) || !File.Exists(executablePath))
            {
                UnityEngine.Debug.LogError("Bugsnag CLI not available. Symbol upload aborted.");
                return false;
            }

            UnityEngine.Debug.Log($"Executing Bugsnag CLI:\n{executablePath} {args}");

            int exitCode = StartProcess(executablePath, args, out string output, out string error);

            if (exitCode != 0)
            {
                UnityEngine.Debug.LogError($"Error uploading symbols: {error}\nOutput: {output}");
                return false;
            }

            return true;
        }

        public static string GetBugsnagCLI()
        {
            try
            {
                EnsureDirectoryExists();

                if (File.Exists(CLI_PATH) && GetCurrentCliVersion() == CLI_VERSION)
                {
                    return CLI_PATH;
                }

                DownloadCLI();
                if (!MakeExecutable())
                {
                    throw new InvalidOperationException("Failed to make CLI executable.");
                }

                return CLI_PATH;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to setup Bugsnag CLI: {ex.Message}");
                return null;
            }
        }

        private static void EnsureDirectoryExists()
        {
            var directory = Path.GetDirectoryName(CLI_PATH);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static void DownloadCLI()
        {
            string url = GetDownloadUrl();
            if (string.IsNullOrEmpty(url))
            {
                throw new InvalidOperationException($"Unsupported platform: {RuntimeInformation.OSDescription}");
            }

            string tempPath = CLI_PATH + ".tmp";

            try
            {
                int exitCode = StartProcess("curl", $"-L {url} --output {tempPath}", out string output, out string error);
                if (exitCode != 0)
                {
                    throw new InvalidOperationException($"Download failed: {error}");
                }

                if (File.Exists(CLI_PATH))
                {
                    File.Delete(CLI_PATH);
                }

                File.Move(tempPath, CLI_PATH);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        private static string GetDownloadUrl()
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

            return fileName != null ? BASE_DOWNLOAD_URL + fileName : null;
        }

        private static bool MakeExecutable()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                int exitCode = StartProcess("chmod", $"+x {CLI_PATH}", out _, out _);
                return exitCode == 0;
            }
            return true;
        }

        private static string GetCurrentCliVersion()
        {
            if (!File.Exists(CLI_PATH))
            {
                UnityEngine.Debug.LogError($"Bugsnag CLI not found at {CLI_PATH}");
                return null;
            }

            int exitCode = StartProcess(CLI_PATH, "--version", out string output, out string error);
            if (exitCode != 0)
            {
                UnityEngine.Debug.LogError($"Error checking CLI version: {error}");
                return null;
            }

            return output.Trim();
        }

        private static int StartProcess(string fileName, string arguments, out string standardOutput, out string standardError)
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