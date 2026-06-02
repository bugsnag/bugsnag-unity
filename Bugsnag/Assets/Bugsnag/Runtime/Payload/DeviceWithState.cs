using System;
using System.Collections.Generic;
using UnityEngine;
#nullable enable

#if ENABLE_WINMD_SUPPORT && UNITY_WSA && !UNITY_EDITOR
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
#endif

namespace BugsnagUnity.Payload
{
    public class DeviceWithState : Device, IDeviceWithState
    {

        private const string FREE_DISK_KEY = "freeDisk";
        private const string FREE_MEMORY_KEY = "freeMemory";
        private const string ORIENTATION_KEY = "orientation";
        private const string TIME_KEY = "time";

        public long? FreeDisk
        {
            get => (long?)Get(FREE_DISK_KEY);
            set => Add(FREE_DISK_KEY, value);
        }

        public long? FreeMemory
        {
            get => (long?)Get(FREE_MEMORY_KEY);
            set => Add(FREE_MEMORY_KEY, value);
        }

        public string? Orientation
        {
            get => (string?)Get(ORIENTATION_KEY);
            set => Add(ORIENTATION_KEY, value);
        }

        public DateTimeOffset? Time
        {
            get => (DateTimeOffset?)Get(TIME_KEY);
            set => Add(TIME_KEY, value);
        }

        internal DeviceWithState(Dictionary<string, object> cachedData) : base(cachedData) { }

        internal DeviceWithState(Configuration configuration, string deviceId) : base(configuration, deviceId)
        {
            Orientation = Input.deviceOrientation.ToString();
            Time = DateTimeOffset.Now;
            Id = deviceId;
            PopulateUwpState();
        }

        private void PopulateUwpState()
        {
#if ENABLE_WINMD_SUPPORT && UNITY_WSA && !UNITY_EDITOR
            switch (Application.platform)
            {
                case RuntimePlatform.WSAPlayerARM:
                case RuntimePlatform.WSAPlayerX64:
                case RuntimePlatform.WSAPlayerX86:
                    TryPopulateUwpFreeMemory();
                    TryPopulateUwpFreeDisk();
                    break;
            }
#endif
        }

#if ENABLE_WINMD_SUPPORT && UNITY_WSA && !UNITY_EDITOR
        private void TryPopulateUwpFreeMemory()
        {
            try
            {
                ulong appMemoryLimit = MemoryManager.AppMemoryUsageLimit;
                ulong appMemoryUsage = MemoryManager.AppMemoryUsage;

                if (appMemoryLimit >= appMemoryUsage && appMemoryLimit <= long.MaxValue)
                {
                    FreeMemory = (long)(appMemoryLimit - appMemoryUsage);
                }
            }
            catch
            {
            }
        }

        private const string UWP_SYSTEM_FREE_SPACE = "System.FreeSpace";

        private void TryPopulateUwpFreeDisk()
        {
            try
            {
                var properties = Task.Run(async () =>
                    await ApplicationData.Current.LocalFolder.Properties
                        .RetrievePropertiesAsync(new[] { UWP_SYSTEM_FREE_SPACE })
                        .AsTask()
                ).GetAwaiter().GetResult();

                if (properties != null
                    && properties.TryGetValue(UWP_SYSTEM_FREE_SPACE, out object freeDisk)
                    && TryConvertToLong(freeDisk, out long freeDiskValue))
                {
                    FreeDisk = freeDiskValue;
                }
            }
            catch
            {
            }
        }

        private static bool TryConvertToLong(object value, out long result)
        {
            switch (value)
            {
                case ulong ulongValue when ulongValue <= long.MaxValue:
                    result = (long)ulongValue;
                    return true;
                case long longValue when longValue >= 0:
                    result = longValue;
                    return true;
                case uint uintValue:
                    result = uintValue;
                    return true;
                case int intValue when intValue >= 0:
                    result = intValue;
                    return true;
                default:
                    result = 0;
                    return false;
            }
        }
#endif


    }
}
