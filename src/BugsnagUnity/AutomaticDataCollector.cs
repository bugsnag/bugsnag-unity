using System;
using System.Collections.Generic;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    internal class AutomaticDataCollector
    {

        internal static void AddAutomaticData(Metadata metadata)
        {
            // data added to metadata.app
            var appMetadata = new Dictionary<string, object>();
            appMetadata.Add("companyName", Application.companyName);
            appMetadata.Add("name", Application.productName);
            metadata.Add("app",appMetadata);

            //data added to metadata.device
            var deviceMetadata = new Dictionary<string, object>();
            if (SystemInfo.batteryLevel > -1)
            {
                deviceMetadata.Add("batteryLevel", SystemInfo.batteryLevel);
            }
            deviceMetadata.Add("charging", SystemInfo.batteryStatus.Equals(BatteryStatus.Charging));
            deviceMetadata.Add("screenDensity", Screen.dpi);
            var res = Screen.currentResolution;
            deviceMetadata.Add("screenResolution", string.Format("{0}x{1}", res.width, res.height));
            deviceMetadata.Add("graphicsDeviceVersion", SystemInfo.graphicsDeviceVersion);
            deviceMetadata.Add("graphicsMemorySize", SystemInfo.graphicsMemorySize.ToString());
            deviceMetadata.Add("graphicsShaderLevel", SystemInfo.graphicsShaderLevel.ToString());
            var processorType = SystemInfo.processorType;
            if (!string.IsNullOrEmpty(processorType))
            {
                deviceMetadata.Add("processorType", processorType);
            }
            deviceMetadata.Add("osLanguage", Application.systemLanguage.ToString());
            metadata.Add("device", deviceMetadata);
        }

    }
}
