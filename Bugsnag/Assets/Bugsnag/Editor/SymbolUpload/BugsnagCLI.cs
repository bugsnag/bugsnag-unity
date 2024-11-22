using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace BugsnagUnity.Editor
{
    internal class BugsnagCLI
    {

        public static bool UploadAndroidSymbols(string customExecutablePath, string buildOutputPath, string apiKey, string versionName, int versionCode)
        {

            string args = $"upload unity-android --api-key={apiKey} --verbose --project-root={Application.dataPath} {buildOutputPath}";

            if (!string.IsNullOrEmpty(versionName))
                args += $" --version-name={versionName}";
            if (versionCode > -1)
                args += $" --version-code={versionCode}";

            return RunCLICommand(customExecutablePath, args, out string output);

        }

        private static bool RunCLICommand(string customExecutablePath, string args, out string output)
        {
            output = null;
            string executablePath = File.Exists(customExecutablePath) ? customExecutablePath : BugsnagCLIDownloader.GetbugsnagCLI();

            if (string.IsNullOrEmpty(executablePath) || !File.Exists(executablePath))
            {
                UnityEngine.Debug.LogError("Bugsnag CLI not available. Symbol upload aborted.");
                return false;
            }

            UnityEngine.Debug.Log($"Executing Bugsnag CLI:\n{executablePath} {args}");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                UnityEngine.Debug.LogError($"Error uploading symbols: {error}\nOutput: {output}");
                return false;
            }

            return true;
        }
    }
}