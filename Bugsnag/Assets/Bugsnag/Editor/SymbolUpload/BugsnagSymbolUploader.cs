using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#if UNITY_IOS || UNITY_STANDALONE_OSX
using UnityEditor.iOS.Xcode;
#endif
using UnityEngine;

namespace BugsnagUnity.Editor
{

    internal class BugsnagSymbolUploader : IPostprocessBuildWithReport
    {
        public int callbackOrder => 1;

        private const string DSYM_UPLOAD_SCRIPT_TEMPLATE = @"#!/bin/bash
if [ ""$ACTION"" == ""install"" ]; then
    if ! <CLI_COMMAND>; then
        echo ""Failed to upload dSYMs to BugSnag.""
    else
        echo ""Successfully uploaded dSYMs to BugSnag.""
    fi
fi
";

        private const string DSYM_UPLOAD_BUILD_PHASE_NAME = "BugSnag dSYM Upload";
        private const string DSYM_UPLOAD_SHELL_NAME = "/bin/sh";

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

            if (report.summary.platform == BuildTarget.Android)
            {
                var buildOutputPath = Path.GetDirectoryName(report.summary.outputPath);
                EditorUtility.DisplayProgressBar("BugSnag Symbol Upload", "Uploading Android symbol files", 0.0f);
                try
                {
                    UploadAndroidSymbols(buildOutputPath, config);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to upload Android symbol files to BugSnag. Error: {e.Message}");
                }
                EditorUtility.ClearProgressBar();
            }
            else if (report.summary.platform == BuildTarget.iOS)
            {
                AddCocoaPostBuildScript(report.summary.outputPath, config);
            }
            else if (report.summary.platform == BuildTarget.StandaloneOSX)
            {
                AddCocoaPostBuildScript(GetMacosXcodeProjectPath(report.summary.outputPath), config);
            }

        }

        // The PBX library is built to expect the layout of a unity iphone xcode project, so we have to manually find the macos xcode project path
        string GetMacosXcodeProjectPath(string outputPath)
        {
            string[] parts = outputPath.Split('/');
            string xcprojFile = parts[^1] + ".xcodeproj";
            return outputPath + "/" + xcprojFile;
        }

        private bool IsSupportedPlatform(BuildTarget platform)
        {
            return platform == BuildTarget.Android || platform == BuildTarget.iOS || platform == BuildTarget.StandaloneOSX;
        }

        private void UploadAndroidSymbols(string buildOutputPath, BugsnagSettingsObject config)
        {
            if (!IsAndroidSymbolCreationEnabled())
            {
                Debug.LogError("Cannot upload Android symbols to BugSnag because Android symbol creation is disabled in the Unity Build Settings.");
                return;
            }
            var cli = new BugsnagCLI();
            cli.UploadAndroidSymbols(buildOutputPath, config.ApiKey, config.AppVersion, config.VersionCode, config.UploadEndpoint);
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

        private void AddCocoaPostBuildScript(string pathToBuiltProject, BugsnagSettingsObject config)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX

#if UNITY_IOS
            var pbxProjectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
#endif
#if UNITY_STANDALONE_OSX
            var pbxProjectPath = pathToBuiltProject + "/project.pbxproj";
             if (!File.Exists(pbxProjectPath))
            {
                //Xcode export not enabled, do nothing
                return;
            }
#endif
            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(pbxProjectPath);
#if UNITY_IOS

            var targetGuid = pbxProject.GetUnityMainTargetGuid();
#endif
#if UNITY_STANDALONE_OSX
            var targetGuid = pbxProject.TargetGuidByName(Application.productName);
#endif

            foreach (var guid in pbxProject.GetAllBuildPhasesForTarget(targetGuid))
            {
                if (DSYM_UPLOAD_BUILD_PHASE_NAME == pbxProject.GetBuildPhaseName(guid))
                {
                    var editedProject = RemoveShellScriptPhase(pbxProject.WriteToString(), guid);
                    pbxProject.ReadFromString(editedProject);
                }
            }

            var uploadScript = GetDsymUploadCommand(config);
            pbxProject.AddShellScriptBuildPhase(targetGuid, DSYM_UPLOAD_BUILD_PHASE_NAME, DSYM_UPLOAD_SHELL_NAME, uploadScript);
            pbxProject.WriteToFile(pbxProjectPath);
#endif
        }

        private string RemoveShellScriptPhase(string project, string guid)
        {
            // Search for and remove the phase object from the XML. only match the guid followed by the braces
            var matches = System.Text.RegularExpressions.Regex.Matches(project, guid + @"\s*\/\*\s*\w+\s*\*\/\s*=\s*\{([\s\S]*?)\};");
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (match.Groups[1].Value.Contains(guid))
                {
                    project = project.Replace(match.Groups[0].Value, "");
                }
            }
            return project;
        }

        private string GetDsymUploadCommand(BugsnagSettingsObject config)
        {
            var cli = new BugsnagCLI();
            var command = cli.GetIosDsymUploadCommand(config.ApiKey, config.UploadEndpoint, config.AppVersion);
            return DSYM_UPLOAD_SCRIPT_TEMPLATE.Replace("<CLI_COMMAND>", command);
        }
    }
}