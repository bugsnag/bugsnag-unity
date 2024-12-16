using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
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

        private const string IOS_DSYM_UPLOAD_SCRIPT_TEMPLATE = @"#!/bin/bash
osascript -e 'tell application ""Terminal"" 
    do script ""<COMMAND>""
    activate
end tell'";

        private const string IOS_DSYM_UPLOAD_BUILD_PHASE_NAME = "BugsnagDSYMUpload";
        private const string IOS_DSYM_UPLOAD_SHELL_NAME = "/bin/sh";

        private string _shelScriptPattern = @"\s*\/\*\s*\w+\s*\*\/\s*=\s*\{([\s\S]*?)\};";
        //   private readonly System.Collections.Generic.List<string> IOS_DSYM_UPLOAD_INPUT_FILES_LIST = new System.Collections.Generic.List<string> { "$(DWARF_DSYM_FOLDER_PATH)/$(DWARF_DSYM_FILE_NAME)/Contents/Resources/DWARF/$(TARGET_NAME)" };

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
                string[] parts = report.summary.outputPath.Split('/');
                string xcprojFile = parts[^1] + ".xcodeproj";
                var projectPath = report.summary.outputPath + "/" + xcprojFile;
                AddMacOSPostBuildScript(projectPath, config.ApiKey, config.UploadEndpoint);
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
            var cli = new BugsnagCLI();
            var command = cli.GetIosDsymUploadCommand(apiKey, uploadEndpoint, pathToBuiltProject);
            var script = IOS_DSYM_UPLOAD_SCRIPT_TEMPLATE.Replace("<COMMAND>",command);
            ModifyXML(pathToBuiltProject + "/Unity-iPhone.xcodeproj/xcshareddata/xcschemes/Unity-iPhone.xcscheme", script);
#endif
        }

        private void ModifyXML(string path, string script)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Debug.LogError("XML file not found at: " + path);
                return;
            }

            XDocument xdoc;
            try
            {
                xdoc = XDocument.Load(path);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to load XML: " + ex.Message);
                return;
            }

            // Locate the ArchiveAction element
            XElement archiveAction = xdoc.Root.Element("ArchiveAction");
            if (archiveAction == null)
            {
                Debug.LogError("No ArchiveAction element found.");
                return;
            }

            // Ensure PostActions element exists
            XElement postActions = archiveAction.Element("PostActions");
            if (postActions == null)
            {
                // Create new PostActions element
                postActions = new XElement("PostActions");
                archiveAction.Add(postActions);
            }

            // Create the new ExecutionAction we need to add
            XElement newExecutionAction = new XElement("ExecutionAction",
                new XAttribute("ActionType", "Xcode.IDEStandardExecutionActionsCore.ExecutionActionType.ShellScriptAction"),
                new XElement("ActionContent",
                    new XAttribute("title", "Run Script"),
                    new XAttribute("scriptText", script)
                )
            );

            // Append the new ExecutionAction into PostActions
            postActions.Add(newExecutionAction);

            // Save changes back to the same file
            try
            {
                xdoc.Save(path);
                Debug.Log("XML updated and saved successfully.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to save modified XML: " + ex.Message);
            }
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

        private void AddMacOSPostBuildScript(string pathToBuiltProject, string apiKey, string uploadEndpoint)
        {
            var pbxProjectPath = pathToBuiltProject + "/project.pbxproj";
            PBXProject project = new PBXProject();
            project.ReadFromFile(pbxProjectPath);
            string targetGuid = project.GetUnityFrameworkTargetGuid();
            // Add your script to the build phases
            string shellScript = "echo \"Hello, this is a macOS post-build script\"";
            project.AddShellScriptBuildPhase(targetGuid, "Run Script", "/bin/sh", shellScript);
            project.WriteToFile(pbxProjectPath);
        }
    }
}