using System;
using System.Collections.Generic;
using UnityEngine;
#nullable enable

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
        }


    }
}
