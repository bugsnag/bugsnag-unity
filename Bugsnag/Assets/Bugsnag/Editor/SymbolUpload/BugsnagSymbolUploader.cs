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

        private const string IOS_DSYM_UPLOAD_SCRIPT_TEMPLATE = @"
        #!/bin/bash

        # Iterate through all input files
        for ((i=0; i<SCRIPT_INPUT_FILE_COUNT; i++))
        do
            # Dynamically get the input file variable name
            INPUT_FILE_VAR=""SCRIPT_INPUT_FILE_$i""
            INPUT_FILE=${!INPUT_FILE_VAR}

            # Extract path up to and including BugsnagUnity.app.dSYM
            DSYM_PATH=$(echo ""$INPUT_FILE"" | sed 's#/Contents.*##')

            echo ""Uploading dSYM: $DSYM_PATH""

            # Upload the dSYM file
            <CLI_COMMAND>
        done";

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
                AddIosPostBuildScript(report.summary.outputPath, config.ApiKey);
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
            // Prepare the shell script to upload dSYM files
            var cli = new BugsnagCLI();
            var command = cli.GetIosDsymUploadCommand(apiKey);
            var dsymUploadScript = IOS_DSYM_UPLOAD_SCRIPT_TEMPLATE.Replace("<CLI_COMMAND>", command);

            // Get the PBXProject object
            string pbxProjectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(pbxProjectPath);

            // add the upload script to the main target
            string targetGUID = pbxProject.GetUnityMainTargetGuid();
            pbxProject.AddShellScriptBuildPhase(targetGUID, "BugsnagDSYMUpload", "/bin/sh", dsymUploadScript, new System.Collections.Generic.List<string> { "$(DWARF_DSYM_FOLDER_PATH)/$(DWARF_DSYM_FILE_NAME)/Contents/Resources/DWARF/$(TARGET_NAME)" });

            // add the upload script to the unity framework target
            string unityFrameworkGUID = pbxProject.GetUnityFrameworkTargetGuid();
            pbxProject.AddShellScriptBuildPhase(unityFrameworkGUID, "BugsnagDSYMUpload", "/bin/sh", dsymUploadScript, new System.Collections.Generic.List<string> { "$(DWARF_DSYM_FOLDER_PATH)/$(DWARF_DSYM_FILE_NAME)/Contents/Resources/DWARF/$(TARGET_NAME)" });

            pbxProject.WriteToFile(pbxProjectPath);

        }
    }
}