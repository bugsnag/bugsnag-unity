#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity
{
    internal class NativeDevice: NativePayloadClassWrapper, IDevice
    {
        public NativeDevice(AndroidJavaObject nativePointer) : base (nativePointer){}

        public string BrowserName { get; set; }
        public string BrowserVersion { get; set; }
        public string ModelNumber { get; set; }
        public string UserAgent { get; set; }

        public string Id { get => GetNativeString("getId"); set => SetNativeString("setId", value); }
        public string Locale { get => GetNativeString("getLocale"); set => SetNativeString("setLocale", value); }
        public string Manufacturer { get => GetNativeString("getManufacturer"); set => SetNativeString("setManufacturer", value); }
        public string Model { get => GetNativeString("getModel"); set => SetNativeString("setModel", value); }
        public string OsName { get => GetNativeString("getOsName"); set => SetNativeString("setOsName", value); }
        public string OsVersion { get => GetNativeString("getOsVersion"); set => SetNativeString("setOsVersion", value); }
        public long? TotalMemory { get => GetNativeLong("getTotalMemory"); set => SetNativeLong("setTotalMemory", value); }
        public bool? Jailbroken { get => GetNativeBool("getJailbroken"); set => SetNativeBool("setJailbroken", value); }

        public string[] CpuAbi { get => GetNativeStringArray("getCpuAbi"); set => SetNativeStringArray("setCpuAbi",value); }

        public IDictionary<string, object> RuntimeVersions { get => GetNativeDictionary("getRuntimeVersions"); set => SetNativeDictionary("setRuntimeVersions", value); }
    }
}
#endif