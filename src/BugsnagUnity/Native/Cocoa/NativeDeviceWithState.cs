using System;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public class NativeDeviceWithState : NativeDevice, IDeviceWithState
    {

        private const string FREE_DISK_KEY = "freeDisk";
        private const string FREE_MEMORY_KEY = "freeMemory";
        private const string ORIENTATION_KEY = "orientation";
        private const string TIME_KEY = "time";


        public NativeDeviceWithState(IntPtr nativeDevice) : base(nativeDevice)
        {
        }

        public ulong? FreeDisk { get => (ulong?)NativeWrapper.GetNativeLong(FREE_DISK_KEY); set => NativeWrapper.SetNativeLong(FREE_DISK_KEY, (long)value); }
        public ulong? FreeMemory { get => (ulong?)NativeWrapper.GetNativeLong(FREE_MEMORY_KEY); set => NativeWrapper.SetNativeLong(FREE_MEMORY_KEY, (long)value); }
        public string Orientation { get => NativeWrapper.GetNativeString(ORIENTATION_KEY); set => NativeWrapper.SetNativeString(ORIENTATION_KEY,value); }
        public DateTime? Time { get => DateTime.Parse(NativeWrapper.GetNativeString(TIME_KEY)); set => NativeWrapper.SetNativeString(TIME_KEY, value?.ToLongTimeString()); }
    }
}
