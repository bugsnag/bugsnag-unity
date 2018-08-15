#if UNITY_5_3_OR_NEWER || UNITY_5
#define UNITY_5_OR_NEWER
#endif
using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class BugsnagBuilder : MonoBehaviour {
    // Thanks to https://gist.github.com/tenpn/f8da1b7df7352a1d50ff for inspiration for this code.
    [PostProcessBuild(1400)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
#if UNITY_5_OR_NEWER
        if (target != BuildTarget.iOS && target != BuildTarget.tvOS && target != BuildTarget.WebGL) {
            return;
        }

        if (target == BuildTarget.WebGL) {

        // Read the index.html file and replace it line by line
        var indexPath = Path.Combine (path, "index.html");
        var indexLines = File.ReadAllLines  (indexPath);
        var sbWeb = new StringBuilder ();
        foreach (var line in indexLines) {
            sbWeb.AppendLine (line);
        }
        File.WriteAllText(indexPath, sbWeb.ToString());
        return;
    }
#else
    if (target != BuildTarget.iPhone) {
        return;
    }
#endif

#if UNITY_IOS
        var scriptUUID = getUUIDForPbxproj ();

        var projectPath = PBXProject.GetPBXProjectPath(path);
        var project = new PBXProject();
        project.ReadFromFile(projectPath);
        var targetName = PBXProject.GetUnityTargetName();

        project.AddBuildProperty(project.TargetGuidByName(targetName), "OTHER_LDFLAGS", "-ObjC");
        project.WriteToFile(projectPath);

        var xcodeProjectPath = Path.Combine (path, "Unity-iPhone.xcodeproj");
        var pbxPath = Path.Combine (xcodeProjectPath, "project.pbxproj");

        var sb = new StringBuilder ();

        var xcodeProjectLines = File.ReadAllLines (pbxPath);
        var inBuildPhases = false;

        var needsBugsnagScript = true;
        foreach (var line in xcodeProjectLines) {
            if (line.Contains ("bugsnag dsym upload script")) {
                needsBugsnagScript = false;
            }
        }


        foreach (var line in xcodeProjectLines) {
            // Enable objective C exceptions
            if (line.Contains("GCC_ENABLE_OBJC_EXCEPTIONS") ||
                       line.Contains ("GCC_ENABLE_CPP_EXCEPTIONS")) {
                var newLine = line.Replace("NO", "YES");
                Debug.Log(line);
                sb.AppendLine(newLine);

            } else if (needsBugsnagScript && line.Contains ("/* Begin PBXResourcesBuildPhase section */")) {
                sb.AppendLine (line);

                sb.Append (
                    "\t\t" + scriptUUID + " /* ShellScript */ = {\n" +
                    "\t\t\tisa = PBXShellScriptBuildPhase;\n" +
                    "\t\t\tbuildActionMask = 2147483647;\n" +
                    "\t\t\tfiles = (\n" +
                    "\t\t\t);\n" +
                    "\t\t\tinputPaths = (\n" +
                    "\t\t\t);\n" +
                    "\t\t\toutputPaths = (\n" +
                    "\t\t\t);\n" +
                    "\t\t\trunOnlyForDeploymentPostprocessing = 0;\n" +
                    "\t\t\tshellPath = \"/usr/bin/env ruby\";\n" +
                    "\t\t\tshellScript = \"# bugsnag dsym upload script\\nfork do\\n  Process.setsid\\n  STDIN.reopen(\\\"/dev/null\\\")\\n  STDOUT.reopen(\\\"/dev/null\\\", \\\"a\\\")\\n  STDERR.reopen(\\\"/dev/null\\\", \\\"a\\\")\\n\\n  require \\\"shellwords\\\"\\n\\n  Dir[\\\"#{ENV[\\\"DWARF_DSYM_FOLDER_PATH\\\"]}/*/Contents/Resources/DWARF/*\\\"].each do |dsym|\\n    system(\\\"curl -F dsym=@#{Shellwords.escape(dsym)} -F projectRoot=#{Shellwords.escape(ENV[\\\"PROJECT_DIR\\\"])} https://upload.bugsnag.com/\\\")\\n  end\\nend\";\n" +
                    "\t\t};\n"
                );
            } else if (needsBugsnagScript && line.Contains ("buildPhases = (")) {
                inBuildPhases = true;
                sb.AppendLine(line);
            } else if (needsBugsnagScript && inBuildPhases && line.Contains(");")) {
                inBuildPhases = false;
                sb.AppendLine ("\t\t\t\t" + scriptUUID + " /* ShellScript */,");
                sb.AppendLine (line);

            } else {
                sb.AppendLine(line);
            }

        }

        File.WriteAllText(pbxPath, sb.ToString());
#endif
    }

    private static string getUUIDForPbxproj() {
        return System.Guid.NewGuid ().ToString ("N").Substring (0, 24).ToUpper ();
    }
}
