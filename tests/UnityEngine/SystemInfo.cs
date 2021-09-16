using System;

namespace UnityEngine
{
    public class SystemInfo
    {
        public static string deviceModel { get; }
        public static string deviceUniqueIdentifier { get; }
        public static string graphicsDeviceVersion { get; }
        public static int graphicsMemorySize { get; }
        public static int graphicsShaderLevel { get; }
        public static int systemMemorySize { get; }
        public static string processorType { get; }
        public static string systemLanguage { get; }
        public static float batteryLevel { get; }
        public static BatteryStatus batteryStatus { get; }
    }
    namespace Events
    {
    }
}
