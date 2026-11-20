using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BugsnagUnity.Editor
{
    /// <summary>
    /// Unity versions without .xcframework plugin support (e.g. 2020.3) treat bugsnag-ios.xcframework
    /// as a plain folder and import each slice's libbugsnag-ios.a as a separate plugin with default
    /// settings ("Any Platform"). Two plugins with the same name then collide when building for
    /// Android or WebGL, and both slices would be linked into iOS builds.
    ///
    /// This restricts the slices to iOS and enables only the one matching the configured SDK
    /// (device or simulator). On Unity versions that import the xcframework as a single plugin the
    /// slices are not assets, so none of this runs.
    /// </summary>
    [InitializeOnLoad]
    public class BugsnagXCFrameworkSlices : AssetPostprocessor, IPreprocessBuildWithReport
    {
        private const string XCFRAMEWORK_DIR = "/bugsnag-ios.xcframework/";

        private static readonly BuildTarget[] NonIOSTargets =
        {
            BuildTarget.Android,
            BuildTarget.WebGL,
            BuildTarget.StandaloneOSX,
            BuildTarget.StandaloneWindows,
            BuildTarget.StandaloneWindows64,
            BuildTarget.StandaloneLinux64,
            BuildTarget.tvOS,
        };

        public int callbackOrder => 0;

        static BugsnagXCFrameworkSlices()
        {
            // Fix projects whose slices were already imported with default settings.
            // AssetDatabase can't be modified during editor load, so defer to the first editor tick.
            EditorApplication.delayCall += ConfigureAllSlices;
        }

        // Runs on first import, so fresh package imports get the right settings straight away
        private void OnPreprocessAsset()
        {
            if (IsSlice(assetPath) && assetImporter is PluginImporter importer)
            {
                Configure(importer, IsSimulatorBuild());
            }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            ConfigureAllSlices();
        }

        internal static bool IsSlice(string path)
        {
            return path.Contains(XCFRAMEWORK_DIR) && path.EndsWith(".a", System.StringComparison.Ordinal);
        }

        private static bool IsSimulatorSlice(string path)
        {
            return path.IndexOf("simulator", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsSimulatorBuild()
        {
            return PlayerSettings.iOS.sdkVersion == iOSSdkVersion.SimulatorSDK;
        }

        private static IEnumerable<PluginImporter> FindSlices()
        {
            return PluginImporter.GetAllImporters().Where(importer => IsSlice(importer.assetPath));
        }

        private static void ConfigureAllSlices()
        {
            try
            {
                var simulator = IsSimulatorBuild();
                foreach (var importer in FindSlices())
                {
                    if (Configure(importer, simulator))
                    {
                        importer.SaveAndReimport();
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Bugsnag: failed to configure the bugsnag-ios.xcframework libraries (" + e.Message + "). If your build reports colliding libbugsnag-ios.a plugins, set the libraries inside bugsnag-ios.xcframework to iOS only in their import settings.");
            }
        }

        /// <returns>true if any setting changed</returns>
        private static bool Configure(PluginImporter importer, bool simulator)
        {
            var enableForIOS = IsSimulatorSlice(importer.assetPath) == simulator;
            var changed = importer.GetCompatibleWithAnyPlatform()
                || importer.GetCompatibleWithEditor()
                || importer.GetCompatibleWithPlatform(BuildTarget.iOS) != enableForIOS
                || NonIOSTargets.Any(importer.GetCompatibleWithPlatform);
            if (!changed)
            {
                return false;
            }

            importer.SetCompatibleWithAnyPlatform(false);
            importer.SetCompatibleWithEditor(false);
            foreach (var target in NonIOSTargets)
            {
                importer.SetCompatibleWithPlatform(target, false);
            }
            importer.SetCompatibleWithPlatform(BuildTarget.iOS, enableForIOS);
            return true;
        }
    }
}
