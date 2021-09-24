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
        private const string CHARGING_KEY = "charging";
        private const string BATTERY_LEVEL_KEY = "batteryLevel";


        public ulong? FreeDisk
        {
            get
            {
                if (HasKey(FREE_DISK_KEY))
                {
                    return (ulong)Payload.Get(FREE_DISK_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(FREE_DISK_KEY, value);
        }

        public ulong? FreeMemory
        {
            get
            {
                if (HasKey(FREE_MEMORY_KEY))
                {
                    return (ulong)Payload.Get(FREE_MEMORY_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(FREE_MEMORY_KEY, value);
        }

        public string Orientation
        {
            get
            {
                if (HasKey(ORIENTATION_KEY))
                {
                    return Payload.Get(ORIENTATION_KEY) as string;
                }
                else
                {
                    return null;
                }
            }
            set => Add(ORIENTATION_KEY, value);
        }

        public DateTime? Time
        {
            get
            {
                if (HasKey(TIME_KEY))
                {
                    return (DateTime)Payload.Get(TIME_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(TIME_KEY, value);
        }

        public DeviceWithState(IConfiguration configuration) : base(configuration)
        {
            //callback mutable values
            Orientation = Input.deviceOrientation.ToString();
            Time = DateTime.UtcNow;

            //hidden non-mutable values
            AddBatteryLevel();
            Add(CHARGING_KEY, SystemInfo.batteryStatus.Equals(BatteryStatus.Charging));
        }

        private void AddBatteryLevel()
        {
            if (SystemInfo.batteryLevel > -1)
            {
                Add(BATTERY_LEVEL_KEY, SystemInfo.batteryLevel);
            }
        }
    }
}
