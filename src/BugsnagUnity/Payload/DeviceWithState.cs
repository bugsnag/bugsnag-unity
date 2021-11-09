using System;
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

        public DateTime? Time
        {
            get => (DateTime?)Get(TIME_KEY);
            set => Add(TIME_KEY, value);
        }

        internal DeviceWithState(Configuration configuration) : base(configuration)
        {
            Orientation = Input.deviceOrientation.ToString();
            Time = DateTime.UtcNow;
        }


    }
}
