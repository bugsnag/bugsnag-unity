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

        var scriptUUID = getUUIDForPbxproj ();

        var xcodeProjectPath = Path.Combine (path, "Unity-iPhone.xcodeproj");
        var pbxPath = Path.Combine (xcodeProjectPath, "project.pbxproj");

        var output = new StringBuilder();
        var xcodeProjectLines = File.ReadAllLines(pbxPath).GetEnumerator();

        while (xcodeProjectLines.MoveNext())
        {
            var currentLine = (string)xcodeProjectLines.Current;

            if (currentLine.Contains("GCC_ENABLE_OBJC_EXCEPTIONS"))
            {
                output.AppendLine(currentLine.Replace("NO", "YES"));
            }
            else if (currentLine.Contains("GCC_ENABLE_CPP_EXCEPTIONS"))
            {
                output.AppendLine(currentLine.Replace("NO", "YES"));
            }
            else if (currentLine.Contains("/* Begin PBXResourcesBuildPhase section */"))
            {
                output.AppendLine(currentLine);
                ProcessResourcesBuildPhaseSection(xcodeProjectLines, scriptUUID, output);
            }
            else if (currentLine.Contains("buildPhases = ("))
            {
                output.AppendLine(currentLine);
                ProcessBuildPhase(xcodeProjectLines, scriptUUID, output);
            }
            else if (currentLine.Contains("OTHER_LDFLAGS = ("))
            {
                output.AppendLine(currentLine);
                ProcessLinkerFlags(xcodeProjectLines, output);
            }
            else
            {
                output.AppendLine(currentLine);
            }
        }

        File.WriteAllText(pbxPath, output.ToString());
    }

    private static void ProcessBuildPhase(IEnumerator lines, string uuid, StringBuilder output)
    {
        var needsBuildPhase = true;

        while (lines.MoveNext())
        {
            var currentLine = (string)lines.Current;

            if (currentLine.Contains(uuid))
            {
                needsBuildPhase = false;
                output.AppendLine(currentLine);
            }
            else if (currentLine.Contains(");"))
            {
                if (needsBuildPhase)
                {
                    output.AppendFormat("\t\t\t\t{0} /* ShellScript */,", uuid);
                    output.AppendLine();
                }

                output.AppendLine(currentLine);
                break;
            }
            else
            {
                output.AppendLine(currentLine);
            }
        }
    }

    private static void ProcessResourcesBuildPhaseSection(IEnumerator lines, string uuid, StringBuilder output)
    {
        var needsBuildPhaseScript = true;

        while (lines.MoveNext())
        {
            var currentLine = (string)lines.Current;

            if (currentLine.Contains("bugsnag dsym upload script"))
            {
                needsBuildPhaseScript = false;
                output.AppendLine(currentLine);
            }
            else if (currentLine.Contains("/* End PBXResourcesBuildPhase section */"))
            {
                if (needsBuildPhaseScript)
                {
                    output.AppendFormat("\t\t{0} /* ShellScript */ = {{", uuid);
                    output.AppendLine();
                    output.AppendLine("\t\t\tisa = PBXShellScriptBuildPhase;");
                    output.AppendLine("\t\t\tbuildActionMask = 2147483647;");
                    output.AppendLine("\t\t\tfiles = (");
                    output.AppendLine("\t\t\t);");
                    output.AppendLine("\t\t\tinputPaths = (");
                    output.AppendLine("\t\t\t);");
                    output.AppendLine("\t\t\toutputPaths = (");
                    output.AppendLine("\t\t\t);");
                    output.AppendLine("\t\t\trunOnlyForDeploymentPostprocessing = 0;");
                    output.AppendLine("\t\t\tshellPath = \"/usr/bin/env ruby\";");
                    output.AppendLine("\t\t\tshellScript = \"# bugsnag dsym upload script\\nfork do\\n  Process.setsid\\n  STDIN.reopen(\\\"/dev/null\\\")\\n  STDOUT.reopen(\\\"/dev/null\\\", \\\"a\\\")\\n  STDERR.reopen(\\\"/dev/null\\\", \\\"a\\\")\\n\\n  require \\\"shellwords\\\"\\n\\n  Dir[\\\"#{ENV[\\\"DWARF_DSYM_FOLDER_PATH\\\"]}/*/Contents/Resources/DWARF/*\\\"].each do |dsym|\\n    system(\\\"curl -F dsym=@#{Shellwords.escape(dsym)} -F projectRoot=#{Shellwords.escape(ENV[\\\"PROJECT_DIR\\\"])} https://upload.bugsnag.com/\\\")\\n  end\\nend\";");
                    output.AppendLine("\t\t};");
                }

                output.AppendLine(currentLine);
                break;
            }
            else
            {
                output.AppendLine(currentLine);
            }
        }
    }

    private static void ProcessLinkerFlags(IEnumerator lines, StringBuilder output)
    {
        bool needsLinkerFlag = true;

        while (lines.MoveNext())
        {
            var currentLine = (string)lines.Current;

            if (currentLine.Contains("-ObjC"))
            {
                needsLinkerFlag = false;
                output.AppendLine(currentLine);
            }
            else if (currentLine.Contains(");"))
            {
                if (needsLinkerFlag)
                {
                    output.AppendLine("\t\t\t\t\t\"-ObjC\"");
                }

                output.AppendLine(currentLine);
                break;
            }
            else
            {
                output.AppendLine(currentLine);
            }
        }
    }

    private static string getUUIDForPbxproj() {
        return System.Guid.NewGuid ().ToString ("N").Substring (0, 24).ToUpper ();
    }
}
