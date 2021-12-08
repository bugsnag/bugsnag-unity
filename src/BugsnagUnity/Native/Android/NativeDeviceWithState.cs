using System;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    internal class NativeDeviceWithState : NativeDevice, IDeviceWithState
    {
        public NativeDeviceWithState(AndroidJavaObject androidJavaObject) : base (androidJavaObject){}

        public long? FreeDisk { get => GetNativeLong("getFreeDisk"); set => SetNativeLong("setFreeDisk",value); }
        public long? FreeMemory { get => GetNativeLong("getFreeMemory"); set => SetNativeLong("setFreeMemory", value); }
        public string Orientation { get => GetNativeString("getOrientation"); set => SetNativeString("setOrientation", value); }
        public DateTime? Time { get => GetNativeDateTime("getTime"); set => SetNativeDateTime("setTime",value); }
    }
}
