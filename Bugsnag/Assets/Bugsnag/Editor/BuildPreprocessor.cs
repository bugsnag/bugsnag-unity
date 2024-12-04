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
            const string emitIl2cppSourceMapping = "--emit-source-mapping";
            var args = PlayerSettings.GetAdditionalIl2CppArgs();
            if (!args.Contains(emitIl2cppSourceMapping))
            {
                PlayerSettings.SetAdditionalIl2CppArgs(args + emitIl2cppSourceMapping);
            }
        }
    }
}