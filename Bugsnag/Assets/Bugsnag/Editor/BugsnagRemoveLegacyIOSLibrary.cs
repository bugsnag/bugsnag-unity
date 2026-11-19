using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BugsnagUnity.Editor
{
    /// <summary>
    /// Importing a .unitypackage never deletes files, so upgrading from a version that shipped
    /// libbugsnag-ios.a leaves it next to bugsnag-ios.xcframework, causing duplicate symbols.
    /// </summary>
    [InitializeOnLoad]
    public class BugsnagRemoveLegacyIOSLibrary
    {
        private const string LEGACY_LIBRARY = "libbugsnag-ios";
        private const string XCFRAMEWORK = "bugsnag-ios";

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
                    .Any(path => path.EndsWith("/bugsnag-ios.xcframework"));
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
                        Debug.Log("Bugsnag: removed obsolete " + path + " (replaced by bugsnag-ios.xcframework)");
                    }
                    else
                    {
                        Debug.LogWarning("Bugsnag: could not remove obsolete " + path + ". Please delete it manually to avoid duplicate symbol errors when building for iOS.");
                    }
                }
            }
            catch (System.Exception e)
            {
                // Never block the editor over cleanup, but let the user know what to do
                Debug.LogWarning("Bugsnag: failed to check for the obsolete libbugsnag-ios.a (" + e.Message + "). If Assets/Bugsnag/Plugins/iOS/libbugsnag-ios.a still exists, please delete it manually to avoid duplicate symbol errors when building for iOS.");
            }
        }
    }
}
