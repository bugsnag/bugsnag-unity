#if UNITY_5_3_OR_NEWER || UNITY_5
#define UNITY_5_OR_NEWER
#endif
using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class BugsnagBuilder : MonoBehaviour {

    // We need to enable ARC on all of the Bugsnag files, this is a fairly simple find and replace:
    //
    // D8A1C700dE80637F000160D4 /* Bugsnag.m in Sources */ = {isa = PBXBuildFile; fileRef = D8A1C700dE80637F100160D4 /* Bugsnag.m */; };
    // D8A1C700dE80637F000160D4 /* Bugsnag.m in Sources */ = {isa = PBXBuildFile; fileRef = D8A1C700dE80637F100160D4 /* Bugsnag.m */; settings = {COMPILER_FLAGS = "-fobjc-arc"; }; };
    //
    // We also need to disable ARC on KSZombie.m using -fno-objc-arc
    static string[] BUGSNAG_FILES = {
        "Bugsnag.m",
        "BugsnagConfiguration.m",
        "BugsnagCrashReport.m",
        "BugsnagIosNotifier.m",
        "BugsnagUnity.mm",
        "BugsnagMetaData.m",
        "BugsnagNotifier.m",
        "BugsnagSink.m",
        "KSZombie.m"
    };
    static Regex _matcher = null;

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

        Regex fileMatcher = getFileMatcher ();

        var scriptUUID = getUUIDForPbxproj ();

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
            // Enable / Disable ARC where required
            if (fileMatcher.IsMatch (line)) {
                var index = line.LastIndexOf("}");
                var newLine = "";
                // Disable ARC for KSZombie.m only
                if (line.Contains("KSZombie.m")) {
                    newLine = line.Substring (0, index) + "settings = {COMPILER_FLAGS = \"-fno-objc-arc\"; }; " + line.Substring(index);
                } else {
                    newLine = line.Substring (0, index) + "settings = {COMPILER_FLAGS = \"-fobjc-arc\"; }; " + line.Substring(index);
                }


                sb.AppendLine(newLine);

            // Enable objective C exceptions
            } else if (line.Contains("GCC_ENABLE_OBJC_EXCEPTIONS") ||
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
    }

    private static string getUUIDForPbxproj() {
        return System.Guid.NewGuid ().ToString ("N").Substring (0, 24).ToUpper ();
    }

    // Regex to try and find lines identifying the bugsnag source files in the Xcode project.
    private static Regex getFileMatcher() {

        if (_matcher == null) {
            var sb = new StringBuilder();
            sb.Append ("( ");

            for (int i = 0; i < BUGSNAG_FILES.Length; i++) {
                if (i > 0) {
                    sb.Append (" | ");
                }
                sb.Append(Regex.Escape(BUGSNAG_FILES[i]));
            }

            sb.Append (" )");

            _matcher = new Regex("isa = PBXBuildFile.*" + sb.ToString() + "");
        }
        return _matcher;
    }
}
