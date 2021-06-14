using UnityEngine;
using System.Collections.Generic;

namespace BugsnagUnity
{
    class UnityMetadata
    {
        internal static Dictionary<string, string> DefaultAppMetadata = new Dictionary<string, string>();

        internal static Dictionary<string, string> DefaultDeviceMetadata = new Dictionary<string, string>();

        internal static void InitDefaultMetadata()
        {
            InitAppMetadata();
            InitDeviceMetadata();
        }

        private static void InitAppMetadata()
        {
            DefaultAppMetadata.Add("companyName", Application.companyName);
            DefaultAppMetadata.Add("name", Application.productName);
        }

        private static void InitDeviceMetadata()
        {
            DefaultDeviceMetadata.Add("graphicsDeviceVersion", SystemInfo.graphicsDeviceVersion);
            DefaultDeviceMetadata.Add("graphicsMemorySize", SystemInfo.graphicsMemorySize.ToString());
            DefaultDeviceMetadata.Add("graphicsShaderLevel", SystemInfo.graphicsShaderLevel.ToString());
            var processorType = SystemInfo.processorType;
            if (!string.IsNullOrEmpty(processorType))
            {
                DefaultDeviceMetadata.Add("processorType", processorType);
            }
            DefaultDeviceMetadata.Add("osLanguage", Application.systemLanguage.ToString());
        }
    }
}
