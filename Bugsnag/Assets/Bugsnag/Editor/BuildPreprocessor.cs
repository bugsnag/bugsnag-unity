using UnityEditor;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class BuildPreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        // This will be enabled once IL2CPP source mapping is supported
        // if (PlayerSettings.GetScriptingBackend(report.summary.platformGroup) != ScriptingImplementation.IL2CPP)
        // {
        //     return;
        // }

        // // Causes il2cpp to generate my-build-dir/Classes/Native/Symbols/LineNumberMappings.json
        // // Note: It used to be stored in the project root dir as:
        // //   my-build-dir_BackUpThisFolder_ButDontShipItWithYourGame/il2cppOutput/Symbols/LineNumberMappings.json
        // const string emitIl2cppSourceMapping = "--emit-source-mapping";

        // var args = PlayerSettings.GetAdditionalIl2CppArgs();
        // if (!args.Contains(emitIl2cppSourceMapping))
        // {
        //     PlayerSettings.SetAdditionalIl2CppArgs(args + emitIl2cppSourceMapping);
        // }
    }
}
