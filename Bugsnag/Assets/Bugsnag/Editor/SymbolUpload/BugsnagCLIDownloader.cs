using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BugsnagUnity.Editor
{

    internal class BugsnagCLIDownloader
    {
        private const string CLI_DOWNLOAD_VERSION = "2.6.2";
        private static string _cliDownloadPath = Path.Combine(Application.dataPath, $"../bugsnag/bin/v{CLI_DOWNLOAD_VERSION}/bugsnag_cli");
        private static string _cliDownloadUrl = $"https://github.com/bugsnag/bugsnag-cli/releases/download/v{CLI_DOWNLOAD_VERSION}/";

        private static string GetBugsnagCliDownloadUrl()
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

        public static string DownloadBugsnagCli()
        {

            RemoveOldVersions();

            if (File.Exists(_cliDownloadPath))
            {
                return _cliDownloadPath;
            }

            var downloadProcess = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "curl",
                    Arguments = $"-L {GetBugsnagCliDownloadUrl()} --output {_cliDownloadPath}",
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
                return null;
            }
            return MakeCLIExecutable() ? _cliDownloadPath : null;
        }

        private static void RemoveOldVersions()
        {
            var directory = Path.GetDirectoryName(_cliDownloadPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

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
        }

        private static bool MakeCLIExecutable()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                var chmodProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "chmod",
                        Arguments = $"+x {_cliDownloadPath}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                chmodProcess.Start();
                chmodProcess.WaitForExit();

                if (chmodProcess.ExitCode != 0)
                {
                    return false;
                }
            }

            return true;
        }

    }
}