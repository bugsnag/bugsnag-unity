using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BugsnagUnity
{
    /// <summary>
    /// WebGL-specific bridge to detect foreground/background state
    /// Following the standard Unity pattern for JavaScript interop
    /// </summary>
    internal static class BugsnagWebGLBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void BugsnagWebGL_Initialize();

        [DllImport("__Internal")]
        private static extern int BugsnagWebGL_IsInForeground();

        [DllImport("__Internal")]
    private static extern double BugsnagWebGL_GetDurationInForegroundMs();
#else
        // Fallback implementations for editor and other platforms
        private static void BugsnagWebGL_Initialize() { }
        private static int BugsnagWebGL_IsInForeground() => 1;
    private static double BugsnagWebGL_GetDurationInForegroundMs() => 0;
#endif

        /// <summary>
        /// Initialize WebGL foreground/background state detection
        /// Sets up event listeners for visibility changes
        /// </summary>
        public static void Initialize()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                BugsnagWebGL_Initialize();
            }
        }

        /// <summary>
        /// Check if the app is currently in foreground
        /// </summary>
        /// <returns>True if in foreground, false if in background</returns>
        public static bool IsInForeground()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                return BugsnagWebGL_IsInForeground() == 1;
            }
            return true;
        }

        /// <summary>
        /// Get total time spent in the foreground on WebGL, in milliseconds.
        /// </summary>
        public static System.TimeSpan GetDurationInForeground()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                var ms = BugsnagWebGL_GetDurationInForegroundMs();
                return System.TimeSpan.FromMilliseconds(ms);
            }
            return System.TimeSpan.Zero;
        }
    }
}
