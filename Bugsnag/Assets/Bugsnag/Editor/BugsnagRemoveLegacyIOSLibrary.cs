using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BugsnagUnity.Editor
{
    /// <summary>
    /// Importing a .unitypackage never deletes files, so upgrading from a version that shipped
    /// libbugsnag-ios.a leaves it next to Bugsnag.xcframework, causing duplicate symbols.
    /// </summary>
    [InitializeOnLoad]
    public class BugsnagRemoveLegacyIOSLibrary
    {
        private const string LEGACY_LIBRARY = "libbugsnag-ios";
        private const string XCFRAMEWORK = "Bugsnag";

        static BugsnagRemoveLegacyIOSLibrary()
        {
            // AssetDatabase can't be modified during editor load, so defer to the first editor tick
            EditorApplication.delayCall += RemoveLegacyLibrary;
        }

        private static void RemoveLegacyLibrary()
        {
            try
            {
                var hasXCFramework = AssetDatabase.FindAssets(XCFRAMEWORK)
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Any(path => path.EndsWith("/Bugsnag.xcframework"));
                if (!hasXCFramework)
                {
                    return;
                }

                var legacyPaths = AssetDatabase.FindAssets(LEGACY_LIBRARY)
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(path => Path.GetFileName(path) == LEGACY_LIBRARY + ".a");
                foreach (var path in legacyPaths)
                {
                    if (AssetDatabase.DeleteAsset(path))
                    {
                        Debug.Log("Bugsnag: removed obsolete " + path + " (replaced by Bugsnag.xcframework)");
                    }
                }
            }
            catch
            {
                // Never block the editor over cleanup
            }
        }
    }
}
