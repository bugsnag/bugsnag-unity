#if ((UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR) || BGS_COCOA_DEV

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

        public long? FreeDisk { get => NativeWrapper.GetNativeLong(FREE_DISK_KEY); set => NativeWrapper.SetNativeLong(FREE_DISK_KEY, value); }

        public long? FreeMemory { get => NativeWrapper.GetNativeLong(FREE_MEMORY_KEY); set => NativeWrapper.SetNativeLong(FREE_MEMORY_KEY, value); }

        public string Orientation { get => NativeWrapper.GetNativeString(ORIENTATION_KEY); set => NativeWrapper.SetNativeString(ORIENTATION_KEY,value); }

        public DateTimeOffset? Time { get => DateTimeOffset.Parse(NativeWrapper.GetNativeString(TIME_KEY)); set => NativeWrapper.SetNativeString(TIME_KEY, value?.ToString("o")); }
    }
}
#endif