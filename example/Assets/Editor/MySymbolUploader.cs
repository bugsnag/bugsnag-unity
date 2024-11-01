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

    const string API_KEY = "227df1042bc7772c321dbde3b31a03c2";

    private static string GetBugsnagCliUrl()
    {
        string baseUrl = "https://github.com/bugsnag/bugsnag-cli/releases/latest/download/";

        // Detect platform
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return RuntimeInformation.OSArchitecture == Architecture.Arm64
                ? baseUrl + "arm64-macos-bugsnag-cli"
                : baseUrl + "x86_64-macos-bugsnag-cli";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return RuntimeInformation.OSArchitecture == Architecture.X86
                ? baseUrl + "i386-windows-bugsnag-cli.exe"
                : baseUrl + "x86_64-windows-bugsnag-cli.exe";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return RuntimeInformation.OSArchitecture == Architecture.X86
                ? baseUrl + "i386-linux-bugsnag-cli"
                : baseUrl + "x86_64-linux-bugsnag-cli";
        }

        throw new PlatformNotSupportedException("Your platform is not supported for Bugsnag CLI.");
    }

    private static bool DownloadBugsnagCli(string cliUrl, string downloadPath)
    {
        UnityEngine.Debug.Log("Starting Bugsnag CLI download process...");

        if (File.Exists(downloadPath))
        {
            UnityEngine.Debug.Log($"File already exists at {downloadPath}, deleting it.");
            File.Delete(downloadPath); // Remove existing file
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

    private static bool MakeExecutable(string filePath)
    {
        if (System.Environment.OSVersion.Platform == PlatformID.Unix || System.Environment.OSVersion.Platform == PlatformID.MacOSX)
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

    private static bool RunBugsnagCliCommand(string cliPath, string arguments)
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
            UnityEngine.Debug.LogError($"Failed to run the Bugsnag CLI tool. Error: {runCliError}");
            return false;
        }

        UnityEngine.Debug.Log($"Bugsnag CLI tool executed successfully. Output: {runCliOutput}");
        return true;
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.Android)
        {
            return;
        }

        string bugsnagCliUrl = GetBugsnagCliUrl();
        string cliDownloadPath = Path.Combine(Application.dataPath, "../bugsnag-cli");

        if (!DownloadBugsnagCli(bugsnagCliUrl, cliDownloadPath) || !MakeExecutable(cliDownloadPath))
        {
            return; // Exit if the download or chmod process failed
        }

        var path = Path.GetDirectoryName(report.summary.outputPath);
        UnityEngine.Debug.Log($"Build output path: {path}");

        var args = string.Format("upload unity-android --api-key={0} --verbose {1}", API_KEY, path);

        RunBugsnagCliCommand(cliDownloadPath, args);
    }

    [MenuItem("Window/BugSnag/UploadIosDSYMs")]
    private static void SelectAndUploadIosSymbols()
    {
        string selectedPath = EditorUtility.OpenFolderPanel("Select a Directory", "", "");

        if (!string.IsNullOrEmpty(selectedPath))
        {
            UnityEngine.Debug.Log($"Directory selected: {selectedPath}");
            string bugsnagCliUrl = GetBugsnagCliUrl();
            string cliDownloadPath = Path.Combine(Application.dataPath, "../bugsnag-cli");

            if (!DownloadBugsnagCli(bugsnagCliUrl, cliDownloadPath) || !MakeExecutable(cliDownloadPath))
            {
                return; // Exit if the download or chmod process failed
            }

            // Customize the CLI command as needed
            var args = $"upload dsym --api-key={API_KEY} --xcode-project {selectedPath}";
            RunBugsnagCliCommand(cliDownloadPath, args);
        }
        else
        {
            UnityEngine.Debug.Log("No directory selected.");
        }
    }

     [MenuItem("Window/BugSnag/UploadAndroidSymbols")]
    private static void SelectAndUploadAndroidSymbols()
    {
      
    }

     [MenuItem("Window/BugSnag/UploadIosDSYMs", validate = true)]
    private static bool ValidateSelectAndUploadIosSymbols()
    {
        return EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
    }

     [MenuItem("Window/BugSnag/UploadAndroidSymbols", validate = true)]
    private static bool ValidateSelectAndUploadAndroidSymbols()
    {
        return EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
    }
}