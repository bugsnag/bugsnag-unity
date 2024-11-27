using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
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

        private const string IOS_DSYM_UPLOAD_BUILD_PHASE_NAME = "BugsnagDSYMUpload";
        private const string IOS_DSYM_UPLOAD_SHELL_NAME = "/bin/sh";

        private string _shelScriptPattern = @"\s*\/\*\s*\w+\s*\*\/\s*=\s*\{([\s\S]*?)\};";
        private readonly System.Collections.Generic.List<string> IOS_DSYM_UPLOAD_INPUT_FILES_LIST = new System.Collections.Generic.List<string> { "$(DWARF_DSYM_FOLDER_PATH)/$(DWARF_DSYM_FILE_NAME)/Contents/Resources/DWARF/$(TARGET_NAME)" };

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
                    UploadAndroidSymbols(buildOutputPath, config.ApiKey, config.AppVersion, config.VersionCode, config.UploadEndpoint);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to upload Android symbol files to BugSnag. Error: {e.Message}");
                }
                EditorUtility.ClearProgressBar();
            }
            else if (report.summary.platform == BuildTarget.iOS)
            {
                AddIosPostBuildScript(report.summary.outputPath, config.ApiKey, config.UploadEndpoint);
            }
            else if (report.summary.platform == BuildTarget.StandaloneOSX)
            {
                // TODO - Add support for uploading macOS symbols
            }

        }

        private bool IsSupportedPlatform(BuildTarget platform)
        {
            return platform == BuildTarget.Android || platform == BuildTarget.iOS || platform == BuildTarget.StandaloneOSX;
        }

        private void UploadAndroidSymbols(string buildOutputPath, string apiKey, string versionName, int versionCode, string uploadEndpoint)
        {
            if (!IsAndroidSymbolCreationEnabled())
            {
                Debug.LogError("Cannot upload Android symbols to BugSnag because Android symbol creation is disabled in the Unity Build Settings.");
                return;
            }
            var cli = new BugsnagCLI();
            cli.UploadAndroidSymbols(buildOutputPath, apiKey, versionName, versionCode, uploadEndpoint);
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

        private void AddIosPostBuildScript(string pathToBuiltProject, string apiKey, string uploadEndpoint)
        {
#if UNITY_IOS
            // Prepare the shell script to upload dSYM files
            var cli = new BugsnagCLI();
            var command = cli.GetIosDsymUploadCommand(apiKey, uploadEndpoint);
            var dsymUploadScript = IOS_DSYM_UPLOAD_SCRIPT_TEMPLATE.Replace("<CLI_COMMAND>", command);

            // Get the PBXProject object
            string pbxProjectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(pbxProjectPath);

            // add the upload script to the main target
            string mainAppTargetGUID = pbxProject.GetUnityMainTargetGuid();
            foreach (var guid in pbxProject.GetAllBuildPhasesForTarget(mainAppTargetGUID))
            {
                if (IOS_DSYM_UPLOAD_BUILD_PHASE_NAME == pbxProject.GetBuildPhaseName(guid))
                {
                    // remove existing build phase
                    var editedProject = RemoveShellScriptPhase(pbxProject.WriteToString(), guid);
                    pbxProject.ReadFromString(editedProject);
                }
            }

            // add the upload script to the unity framework target
            string unityFrameworkGUID = pbxProject.GetUnityFrameworkTargetGuid();
            foreach (var guid in pbxProject.GetAllBuildPhasesForTarget(unityFrameworkGUID))
            {
                if (IOS_DSYM_UPLOAD_BUILD_PHASE_NAME == pbxProject.GetBuildPhaseName(guid))
                {
                    // remove existing build phase
                    var editedProject = RemoveShellScriptPhase(pbxProject.WriteToString(), guid);
                    pbxProject.ReadFromString(editedProject);
                }
            }

            pbxProject.AddShellScriptBuildPhase(mainAppTargetGUID, IOS_DSYM_UPLOAD_BUILD_PHASE_NAME, IOS_DSYM_UPLOAD_SHELL_NAME, dsymUploadScript, IOS_DSYM_UPLOAD_INPUT_FILES_LIST);
            pbxProject.AddShellScriptBuildPhase(unityFrameworkGUID, IOS_DSYM_UPLOAD_BUILD_PHASE_NAME, IOS_DSYM_UPLOAD_SHELL_NAME, dsymUploadScript, IOS_DSYM_UPLOAD_INPUT_FILES_LIST);
            pbxProject.WriteToFile(pbxProjectPath);
#endif
        }

        private string RemoveShellScriptPhase(string project, string guid)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(project, guid + _shelScriptPattern);
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (match.Groups[1].Value.Contains(guid))
                {
                    project = project.Replace(match.Groups[0].Value, "");
                }
            }
            return project;
        }
    }
}