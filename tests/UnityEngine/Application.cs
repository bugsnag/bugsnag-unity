using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine
{
    public class Application
    {
        //
        // Summary:
        //     The version of the Unity runtime used to play the content.
        public static string unityVersion { get; }

        //
        // Summary:
        //     Returns the platform the game is running on (Read Only).
        public static RuntimePlatform platform { get; }

        //
        // Summary:
        //     The language the user's operating system is running in.
        public static SystemLanguage systemLanguage { get; }

        //
        // Summary:
        //     Returns application identifier at runtime. On Apple platforms this is the 'bundleIdentifier'
        //     saved in the info.plist file, on Android it's the 'package' from the AndroidManifest.xml.
        public static string identifier { get; }

        //
        // Summary:
        //     Returns application version number (Read Only).
        public static string version { get; }

        //
        // Summary:
        //     Return application company name (Read Only).
        public static string companyName { get; }

        //
        // Summary:
        //     Returns application product name (Read Only).
        public static string productName { get; }

        public static void logMessageReceivedThreaded() { }

        public static void logMessageReceived() { }

        public static void add_logMessageReceivedThreaded(LogCallback cb) { }

        public static void add_logMessageReceived(LogCallback cb) { }

        public static string persistentDataPath { get; }

        public delegate void LogCallback(string condition, string stackTrace, LogType type);

        public delegate void LowMemoryCallback();

        public static event LowMemoryCallback lowMemory;       

    }
}
