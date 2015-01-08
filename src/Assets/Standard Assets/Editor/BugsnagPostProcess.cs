using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class BugsnagPostProcess : MonoBehaviour {

    // We need to enable ARC on all of the Bugsnag files, this is a fairly simple find and replace:
    //
    // D8A1C700dE80637F000160D4 /* Bugsnag.m in Sources */ = {isa = PBXBuildFile; fileRef = D8A1C700dE80637F100160D4 /* Bugsnag.m */; };
    // D8A1C700dE80637F000160D4 /* Bugsnag.m in Sources */ = {isa = PBXBuildFile; fileRef = D8A1C700dE80637F100160D4 /* Bugsnag.m */; settings = {COMPILER_FLAGS = "-fobjc-arc"; }; };
    static string[] BUGSNAG_FILES = {
        "Bugsnag.m",
        "BugsnagEvent.m",
        "BugsnagMetaData.m",
        "BugsnagNotifier.m",
        "BugsnagUnity.mm",
        "NSDictionary-BSJSON.m",
        "NSMutableDictionary-BSMerge.m",
        "NSNumber-BSDuration.m",
        "NSNumber-BSFileSizes.m",
        "Reachability.m",
        "UIDevice-BSStats.m",
        "UIViewController-BSVisibility.m"
    };
    static Regex _matcher = null;

    // Thanks to https://gist.github.com/tenpn/f8da1b7df7352a1d50ff for inspiration for this code.
    [PostProcessBuild(1400)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target != BuildTarget.iPhone) {
            return;
        }

        Regex fileMatcher = getFileMatcher ();

        var xcodeProjectPath = Path.Combine (path, "Unity-iPhone.xcodeproj");
        var pbxPath = Path.Combine (xcodeProjectPath, "project.pbxproj");

        var sb = new StringBuilder ();

        var xcodeProjectLines = File.ReadAllLines (pbxPath);

        foreach (var line in xcodeProjectLines) {
            // Enable ARC where required
            if (fileMatcher.IsMatch (line)) {
                var index = line.LastIndexOf("}");
                var newLine = line.Substring (0, index) + "settings = {COMPILER_FLAGS = \"-fobjc-arc\"; }; " + line.Substring(index);

                sb.AppendLine(newLine);

            // Enable objective C exceptions
            } else if (line.Contains("GCC_ENABLE_OBJC_EXCEPTIONS")) {
                var newLine = line.Replace("NO", "YES");
                Debug.Log(line);
                sb.AppendLine(newLine);

            } else {
                sb.AppendLine(line);
            }

        }

        File.WriteAllText(pbxPath, sb.ToString());
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
