using System;
using System.Collections.Generic;
using System.Linq;

namespace BugsnagUnity
{
    public class NativeDevice : IDevice
    {

        private const string ID_KEY = "id";
        private const string JAIL_BROKEN_KEY = "jailbroken";
        private const string LOCALE_KEY = "locale";
        private const string MANUFACTURER_KEY = "manufacturer";
        private const string MODEL_KEY = "model";
        private const string OS_NAME_KEY = "osName";
        private const string OS_VERSION_KEY = "osVersion";

        internal NativePayloadClassWrapper NativeWrapper;

        public NativeDevice(IntPtr nativeDevice)
        {
            NativeWrapper = new NativePayloadClassWrapper(nativeDevice);
        }

        public string BrowserName { get; set; }
        public string BrowserVersion { get; set; }
        public string[] CpuAbi { get; set; }
        public int? TotalMemory { get; set; }
        public string UserAgent { get; set; }
        public string ModelNumber { get; set; }



        public string Id { get => NativeWrapper.GetNativeString(ID_KEY); set => NativeWrapper.SetNativeString(ID_KEY,value); }

        public bool? Jailbroken { get => NativeWrapper.GetNativeBool(JAIL_BROKEN_KEY); set => NativeWrapper.SetNativeBool(JAIL_BROKEN_KEY, value); }

        public string Locale { get => NativeWrapper.GetNativeString(LOCALE_KEY); set => NativeWrapper.SetNativeString(LOCALE_KEY, value); }

        public string Manufacturer { get => NativeWrapper.GetNativeString(MANUFACTURER_KEY); set => NativeWrapper.SetNativeString(MANUFACTURER_KEY, value); }

        public string Model { get => NativeWrapper.GetNativeString(MODEL_KEY); set => NativeWrapper.SetNativeString(MODEL_KEY, value); }

        public string OsName { get => NativeWrapper.GetNativeString(OS_NAME_KEY); set => NativeWrapper.SetNativeString(OS_NAME_KEY, value); }

        public string OsVersion { get => NativeWrapper.GetNativeString(OS_VERSION_KEY); set => NativeWrapper.SetNativeString(OS_VERSION_KEY, value); }

        public Dictionary<string, object> RuntimeVersions { get => GetRuntimeVersions(); set => SetRuntimeVersions(value); }


        private Dictionary<string, object> GetRuntimeVersions()
        {
            var versionsString = NativeCode.bugsnag_getRuntimeVersionsFromDevice(NativeWrapper.NativePointer);
            var split = versionsString.Split('|').ToList();
            split.RemoveAt(split.Count - 1);
            foreach (var str in split)
            {
                str.Replace("|",string.Empty);
            }
            var returnDict = new Dictionary<string, object>();
            for (int i = 0; i < split.Count; i+=2)
            {
                returnDict.Add(split[i], split[i + 1]);
            }
            return returnDict;
        }

        private void SetRuntimeVersions(Dictionary<string, object> versions)
        {
            var stringVersions = new List<string>();
            foreach (var item in versions)
            {
                stringVersions.Add(item.Key);
                stringVersions.Add(item.Value.ToString());
            }
            NativeCode.bugsnag_setRuntimeVersionsFromDevice(NativeWrapper.NativePointer, stringVersions.ToArray(), stringVersions.Count);
        }

    }
}
