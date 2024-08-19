using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BugsnagUnity.Payload;
using UnityEngine;
using System.Collections.Specialized;

namespace BugsnagUnity
{
    class NativeClient : INativeClient
    {
        public Configuration Configuration { get; }

        public IBreadcrumbs Breadcrumbs { get; }

        private bool _launchMarkedAsCompleted = false;

        private bool _hasReceivedLowMemoryWarning = false;

        private Metadata _metadata = new Metadata();
        private OrderedDictionary _featureFlags = new OrderedDictionary();

        public NativeClient(Configuration configuration)
        {
            Configuration = configuration;
            Breadcrumbs = new Breadcrumbs(configuration);
            Application.lowMemory += () => { _hasReceivedLowMemoryWarning = true; };
            if (configuration.FeatureFlags != null)
            {
                _featureFlags = configuration.FeatureFlags;
            }
        }

        public void PopulateApp(App app)
        {
        }

        public void PopulateAppWithState(AppWithState app)
        {
            AddIsLaunching(app);
            app.Add("lowMemory", _hasReceivedLowMemoryWarning);
        }

        private void AddIsLaunching(AppWithState app)
        {
            bool isLaunching;
            if (Configuration.LaunchDurationMillis == 0)
            {
                isLaunching = !_launchMarkedAsCompleted;
            }
            else
            {
                isLaunching = app.Duration?.TotalMilliseconds < Configuration.LaunchDurationMillis;
            }
            app.IsLaunching = isLaunching;
        }

        public void PopulateDevice(Device device)
        {
            device.Manufacturer = "Linux";
            device.Model =  SystemInfo.deviceModel;
        }

        // Struct based on https://man7.org/linux/man-pages/man3/statvfs.3.html
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class StatvfsBuffer
        {
            
            public ulong f_bsize;
            public ulong f_frsize;
            public ulong f_blocks;
            public ulong f_bfree;
            public ulong f_bavailable;

            public ulong f_files;
            public ulong f_ffree;
            public ulong f_favail;

            public ulong f_fsid;
            public ulong f_flag;
            public ulong f_namemax;

        }

        [return: MarshalAs(UnmanagedType.SysInt)]
        [DllImport("libc", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int statvfs(string path, [In, Out] StatvfsBuffer lpBuffer);

        public void PopulateDeviceWithState(DeviceWithState device)
        {
            PopulateDevice(device);
            if (Application.platform != RuntimePlatform.LinuxPlayer)
            {
                return;
            }

            // See https://man7.org/linux/man-pages/man5/proc_meminfo.5.html
            StreamReader sr = new StreamReader("/proc/meminfo");
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                // Values are listed in kibibytes
                if (line.StartsWith("MemFree"))
                {
                    device.FreeMemory = long.Parse(new string(line.Where(c => char.IsNumber(c)).ToArray())) * 1024;
                }
                if (line.StartsWith("MemTotal"))
                {
                    device.TotalMemory = long.Parse(new string(line.Where(c => char.IsNumber(c)).ToArray())) * 1024;
                }
            }
            sr.Close();

            // See https://man7.org/linux/man-pages/man3/statvfs.3.html
            StatvfsBuffer buffer = new StatvfsBuffer();
            if (statvfs(Environment.CurrentDirectory, buffer) == 0) {
                device.FreeDisk = (long)buffer.f_bavailable * (long)buffer.f_bsize;
            }
        }

        public void PopulateMetadata(Metadata metadata)
        {
        }

        public void PopulateUser(User user)
        {
        }

        public void SetMetadata(string section, Dictionary<string, object> metadata)
        {
        }

        public void SetSession(Session session)
        {
        }

        public void SetUser(User user)
        {
        }
        public void SetContext(string context)
        {
        }
        public void SetAutoDetectErrors(bool autoDetectErrors)
        {
        }

        public void SetAutoDetectAnrs(bool autoDetectAnrs)
        {
        }

        public void StartSession()
        {
        }

        public void PauseSession()
        {
        }

        public bool ResumeSession()
        {
            return false;
        }

        public void UpdateSession(Session session)
        {
        }

        public Session GetCurrentSession()
        {
            return null;
        }

        public void MarkLaunchCompleted()
        {
            _launchMarkedAsCompleted = true;
        }

        public LastRunInfo GetLastRunInfo()
        {
            return null;
        }

        public void ClearNativeMetadata(string section)
        {
            _metadata.ClearMetadata(section);
        }

        public void ClearNativeMetadata(string section, string key)
        {
            _metadata.ClearMetadata(section, key);
        }

        public void AddNativeMetadata(string section, IDictionary<string, object> data)
        {
            _metadata.AddMetadata(section, data);
        }

        public IDictionary<string, object> GetNativeMetadata()
        {
            return _metadata.Payload;
        }

        public void AddFeatureFlag(string name, string variant = null)
        {
            _featureFlags[name] = variant;
        }

        public void AddFeatureFlags(FeatureFlag[] featureFlags)
        {
            foreach (var flag in featureFlags)
            {
                _featureFlags[flag.Name] = flag.Variant;
            }
        }

        public void ClearFeatureFlag(string name)
        {
            _featureFlags.Remove(name);
        }

        public void ClearFeatureFlags()
        {
            _featureFlags.Clear();
        }

        public bool ShouldAttemptDelivery()
        {
            return true;
        }

        public void RegisterForOnSessionCallbacks()
        {
            // Not Used on this platform
        }
    }
}
