using UnityEditor;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
namespace BugsnagUnity.Editor
{
    public class BuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (BugsnagPlayerSettingsCompat.GetScriptingBackend(report.summary.platformGroup) != ScriptingImplementation.IL2CPP)
            {
                return;
            }
            // Causes il2cpp to generate my-build-dir/Classes/Native/Symbols/LineNumberMappings.json
            // Note: It used to be stored in the project root dir as:
            //   my-build-dir_BackUpThisFolder_ButDontShipItWithYourGame/il2cppOutput/Symbols/LineNumberMappings.json
            const string emit = "--emit-source-mapping";
            var args = (PlayerSettings.GetAdditionalIl2CppArgs() ?? string.Empty).Trim();

            // token-aware check: flag present as a standalone token?
            var hasToken = System.Text.RegularExpressions.Regex.IsMatch(args, @"(?<!\S)--emit-source-mapping(?!\S)");
            if (!hasToken)
            {
                var updated = string.IsNullOrEmpty(args) ? emit : $"{args} {emit}";
                PlayerSettings.SetAdditionalIl2CppArgs(updated);
            }
        }
    }
}