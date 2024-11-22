using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BugsnagUnity.Editor
{
    internal class BugsnagCLIDownloader
    {
        private const string CLI_VERSION = "2.6.2";
        private static readonly string CLI_PATH = Path.Combine(Application.dataPath, "../bugsnag/bin/bugsnag_cli");
        private static readonly string BASE_DOWNLOAD_URL = $"https://github.com/bugsnag/bugsnag-cli/releases/download/v{CLI_VERSION}/";

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
                var process = StartProcess("curl", $"-L {url} --output {tempPath}");
                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"Download failed: {process.StandardError.ReadToEnd()}");
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return BASE_DOWNLOAD_URL +
                    (RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "arm64-macos-bugsnag-cli" : "x86_64-macos-bugsnag-cli");
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return BASE_DOWNLOAD_URL +
                    (RuntimeInformation.OSArchitecture == Architecture.X86 ? "i386-windows-bugsnag-cli.exe" : "x86_64-windows-bugsnag-cli.exe");
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return BASE_DOWNLOAD_URL +
                    (RuntimeInformation.OSArchitecture == Architecture.X86 ? "i386-linux-bugsnag-cli" : "x86_64-linux-bugsnag-cli");
            }

            return null;
        }

        private static bool MakeExecutable()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                var process = StartProcess("chmod", $"+x {CLI_PATH}");
                return process.ExitCode == 0;
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

            var process = StartProcess(CLI_PATH, "--version");
            if (process.ExitCode != 0)
            {
                UnityEngine.Debug.LogError($"Error checking CLI version: {process.StandardError.ReadToEnd()}");
                return null;
            }

            return process.StandardOutput.ReadToEnd().Trim();
        }

        private static Process StartProcess(string fileName, string arguments)
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
            process.WaitForExit();
            return process;
        }
    }
}