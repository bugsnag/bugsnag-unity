using System.Diagnostics;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class MySymbolUploader : IPostprocessBuildWithReport
{
    public int callbackOrder => 1; // Determines the order of callback execution

     private string GetBugsnagCliUrl()
    {
        string baseUrl = "https://github.com/bugsnag/bugsnag-cli/releases/latest/download/";

        // Detect platform
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                return baseUrl + "arm64-macos-bugsnag-cli";
            }
            else
            {
                return baseUrl + "x86_64-macos-bugsnag-cli";
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (RuntimeInformation.OSArchitecture == Architecture.X86)
            {
                return baseUrl + "i386-windows-bugsnag-cli.exe";
            }
            else
            {
                return baseUrl + "x86_64-windows-bugsnag-cli.exe";
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (RuntimeInformation.OSArchitecture == Architecture.X86)
            {
                return baseUrl + "i386-linux-bugsnag-cli";
            }
            else
            {
                return baseUrl + "x86_64-linux-bugsnag-cli";
            }
        }

        throw new PlatformNotSupportedException("Your platform is not supported for Bugsnag CLI.");
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        string bugsnagCliUrl = GetBugsnagCliUrl();
        string cliDownloadPath = Path.Combine(Application.dataPath, "../bugsnag-cli");

        UnityEngine.Debug.Log("Starting Bugsnag CLI download process...");

        if (File.Exists(cliDownloadPath))
        {
            UnityEngine.Debug.Log($"File already exists at {cliDownloadPath}, deleting it.");
            File.Delete(cliDownloadPath); // Remove existing file
        }

        UnityEngine.Debug.Log($"Downloading Bugsnag CLI from {bugsnagCliUrl} to {cliDownloadPath}...");

        var downloadProcess = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "curl",
                Arguments = $"-L {bugsnagCliUrl} --output {cliDownloadPath}",
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
            return;
        }
        UnityEngine.Debug.Log($"Bugsnag CLI tool downloaded successfully. Output: {downloadOutput}");

        if (System.Environment.OSVersion.Platform == PlatformID.Unix || System.Environment.OSVersion.Platform == PlatformID.MacOSX)
        {
            UnityEngine.Debug.Log($"Making the Bugsnag CLI executable with chmod on Unix/MacOS platform...");

            var chmodProcess = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"+x {cliDownloadPath}",
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
                return;
            }
            UnityEngine.Debug.Log($"Bugsnag CLI made executable successfully. Output: {chmodOutput}");
        }

        var path = Path.GetDirectoryName(report.summary.outputPath);
        UnityEngine.Debug.Log($"Build output path: {path}");

        var apiKey = "227df1042bc7772c321dbde3b31a03c2";

        // Add verbose flag to increase logging output from the CLI tool
        var args = string.Format("upload unity-android --api-key={0} --verbose {1}", apiKey, path);
        UnityEngine.Debug.Log("CLI arguments: " + args);

        // Check if the CLI file exists and is executable before running it
        if (!File.Exists(cliDownloadPath))
        {
            UnityEngine.Debug.LogError($"Bugsnag CLI not found at {cliDownloadPath}");
            return;
        }



        // Run the Bugsnag CLI tool
        UnityEngine.Debug.Log("Running Bugsnag CLI tool...");

        var runCliProcess = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = cliDownloadPath,
                Arguments = args,
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

        // Log the exit code for more context on the failure
        UnityEngine.Debug.Log($"Bugsnag CLI process exited with code {runCliProcess.ExitCode}");

        if (runCliProcess.ExitCode != 0)
        {
            UnityEngine.Debug.LogError($"Failed to run the Bugsnag CLI tool. Error: {runCliError}");
        }
        else
        {
            UnityEngine.Debug.Log($"Bugsnag CLI tool executed successfully. Output: {runCliOutput}");
        }
    }
}