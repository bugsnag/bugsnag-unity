using System;
using System.Collections.Generic;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    internal class AutomaticDataCollector
    {

        internal static void SetDefaultData(INativeClient nativeClient)
        {
            // data added to metadata.app
            var appMetadata = new Dictionary<string, string>();
            appMetadata.Add("companyName", Application.companyName);
            appMetadata.Add("name", Application.productName);
            nativeClient.SetMetadata("app",appMetadata);


            //data added to metadata.device
            var deviceMetadata = new Dictionary<string, string>();
           
            deviceMetadata.Add("screenDensity", Screen.dpi.ToString());

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
            nativeClient.SetMetadata("device", deviceMetadata);

        }

        internal static void AddStatefulDeviceData(Metadata metadata)
        {

            // This is temporary code to make existing tests pass, this will be changed before v6 release when the Metadata class is rewritten

            //data added to metadata.device
            var deviceMetadata = new Dictionary<string, object>();
            if (SystemInfo.batteryLevel > -1)
            {
                deviceMetadata.Add("batteryLevel", SystemInfo.batteryLevel);
            }
            deviceMetadata.Add("charging", SystemInfo.batteryStatus.Equals(BatteryStatus.Charging));

            if (metadata.ContainsKey("device"))
            {
                var existingMetadata = metadata.Get("device");

                if (existingMetadata is Dictionary<string, object>)
                {
                    var castExistingMetadata = (Dictionary<string, object>)existingMetadata;
                    foreach (var item in deviceMetadata)
                    {
                        if (!castExistingMetadata.ContainsKey(item.Key))
                        {
                            castExistingMetadata.Add(item.Key, item.Value);
                        }
                    }
                }
                else if(existingMetadata is Dictionary<string, string>)
                {
                    var castExistingMetadata = (Dictionary<string, string>)existingMetadata;
                    foreach (var item in deviceMetadata)
                    {
                        if (!castExistingMetadata.ContainsKey(item.Key))
                        {
                            castExistingMetadata.Add(item.Key, item.Value.ToString());
                        }
                    }
                }

                
            }
            else
            {
                metadata.Add("device", deviceMetadata);
            }
        }

    }
}
