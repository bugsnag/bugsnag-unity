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
            string dsymUploadScript = File.ReadAllText(_iosUploadScriptPath);
            var command = cli.GetIosDsymUploadCommand(apiKey);
            dsymUploadScript = dsymUploadScript.Replace("#<INSERT_COMMAND>", command);

            string pbxProjectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(pbxProjectPath);

            string targetGUID = pbxProject.GetUnityMainTargetGuid();

                        // Set Debug Information Format for the desired build configurations
            pbxProject.SetBuildProperty(targetGUID, "DEBUG_INFORMATION_FORMAT", "dwarf-with-dsym"); // For all configurations
            // pbxProject.SetBuildPropertyForConfig(pbxProject.BuildConfigByName(targetGUID, "Debug"), "DEBUG_INFORMATION_FORMAT", "dwarf-with-dsym");
            // pbxProject.SetBuildPropertyForConfig(pbxProject.BuildConfigByName(targetGUID, "Release"), "DEBUG_INFORMATION_FORMAT", "dwarf-with-dsym");
            // pbxProject.SetBuildPropertyForConfig(pbxProject.BuildConfigByName(targetGUID, "ReleaseForRunning"), "DEBUG_INFORMATION_FORMAT", "dwarf-with-dsym");
            // pbxProject.SetBuildPropertyForConfig(pbxProject.BuildConfigByName(targetGUID, "ReleaseForProfiling"), "DEBUG_INFORMATION_FORMAT", "dwarf-with-dsym");


            // Add your shell script
            string shellScriptName = "BugsnagDSYMUpload";
            pbxProject.AddShellScriptBuildPhase(targetGUID, shellScriptName, "/bin/sh", dsymUploadScript);

            // Save changes
            pbxProject.WriteToFile(pbxProjectPath);

        }




        

       
    }
}