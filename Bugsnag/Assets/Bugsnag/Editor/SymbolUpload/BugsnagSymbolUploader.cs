using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace BugsnagUnity.Editor
{

    internal class BugsnagSymbolUploader : IPostprocessBuildWithReport
    {
        public int callbackOrder => 1;

        private string _iosUploadScriptPath = Application.dataPath + "/Bugsnag/Editor/SymbolUpload/iosSymbolUpload.sh";


        public void OnPostprocessBuild(BuildReport report)
        {
            if (!IsSupportedPlatform(report.summary.platform))
            {
                return;
            }

            var config = BugsnagSettingsObject.GetSettingsObject();
            if (config == null || !config.AutoUploadSymbols)
            {
                return;
            }

            Debug.Log($"Starting BugSnag symbol upload for {report.summary.platform}...");


            if (report.summary.platform == BuildTarget.Android)
            {
                var buildOutputPath = Path.GetDirectoryName(report.summary.outputPath);
                EditorUtility.DisplayProgressBar("BugSnag Symbol Upload", "Uploading Android symbol files", 0.0f);
                try
                {
                    UploadAndroidSymbols(buildOutputPath, config.ApiKey, config.AppVersion, config.VersionCode);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to upload Android symbol files to BugSnag. Error: {e.Message}");
                }
                EditorUtility.ClearProgressBar();
            }
            else if (report.summary.platform == BuildTarget.iOS)
            {
                AddIosPostBuildScript(report.summary.outputPath,config.ApiKey);
            }
            else if (report.summary.platform == BuildTarget.StandaloneOSX)
            {
                // TODO implement macOS post build script generation
            }

        }

        private bool IsSupportedPlatform(BuildTarget platform)
        {
            return platform == BuildTarget.Android || platform == BuildTarget.iOS || platform == BuildTarget.StandaloneOSX;
        }

        private void UploadAndroidSymbols(string buildOutputPath, string apiKey, string versionName, int versionCode)
        {
            if (!IsAndroidSymbolCreationEnabled())
            {
                Debug.LogError("Cannot upload Android symbols to BugSnag because Android symbol creation is disabled in the Unity Build Settings.");
                return;
            }
            var cli = new BugsnagCLI();
            cli.UploadAndroidSymbols(buildOutputPath, apiKey, versionName, versionCode);
        }

        private bool IsAndroidSymbolCreationEnabled()
        {
#if UNITY_ANDROID

#if UNITY_2021_1_OR_NEWER
            return EditorUserBuildSettings.androidCreateSymbols == AndroidCreateSymbols.Public ||
                   EditorUserBuildSettings.androidCreateSymbols == AndroidCreateSymbols.Debugging;
#else
            return EditorUserBuildSettings.androidCreateSymbolsZip;
#endif
#endif
            return false;
        }


        private void AddIosPostBuildScript(string pathToBuiltProject, string apiKey)
        {

            var cli = new BugsnagCLI();
            string script = File.ReadAllText(_iosUploadScriptPath);
            var command = cli.GetIosDsymUploadCommand(apiKey);
            script = script.Replace("#<INSERT_COMMAND>", command);

            File.WriteAllText(pathToBuiltProject + "/bugsnagDsymUpload.sh", script);

            cli.MakeFileExecutable(pathToBuiltProject + "/bugsnagDsymUpload.sh");

            string pbxProjectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(pbxProjectPath);

            string targetGUID = pbxProject.GetUnityMainTargetGuid();

            // Add your shell script
            string shellScriptName = "BugsnagDSYMUpload";
            string shellScript = pathToBuiltProject + "/bugsnagDsymUpload.sh"; // Change to the actual script path or command
            pbxProject.AddShellScriptBuildPhase(targetGUID, shellScriptName, "/bin/sh", shellScript);

            // Save changes
            pbxProject.WriteToFile(pbxProjectPath);
        }

       
    }
}