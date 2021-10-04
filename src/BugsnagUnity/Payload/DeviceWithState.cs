using System;
using UnityEngine;

namespace BugsnagUnity.Payload
{
    public class DeviceWithState : Device
    {

        private const string FREE_DISK_KEY = "freeDisk";
        private const string FREE_MEMORY_KEY = "freeMemory";
        private const string ORIENTATION_KEY = "orientation";
        private const string TIME_KEY = "time";
    
        public ulong? FreeDisk
        {
            get => (ulong?)Get(FREE_DISK_KEY);
            set => Add(FREE_DISK_KEY, value);
        }

        public ulong? FreeMemory
        {
            get => (ulong?)Get(FREE_MEMORY_KEY);
            set => Add(FREE_MEMORY_KEY, value);
        }

        public string Orientation
        {
            get => (string)Get(ORIENTATION_KEY);
            set => Add(ORIENTATION_KEY, value);
        }

        public DateTime? Time
        {
            get => (DateTime?)Get(TIME_KEY);
            set => Add(TIME_KEY, value);
        }

        internal DeviceWithState(IConfiguration configuration) : base(configuration)
        {
            Orientation = Input.deviceOrientation.ToString();
            Time = DateTime.UtcNow;
        }

       
    }
}
