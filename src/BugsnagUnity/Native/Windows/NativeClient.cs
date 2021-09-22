using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    class NativeClient : INativeClient
    {
        public IConfiguration Configuration { get; }

        public IBreadcrumbs Breadcrumbs { get; }

        public IDelivery Delivery { get; }

        private bool _launchMarkedAsCompleted = false;

        private bool _hasReceivedLowMemoryWarning = false;

        public NativeClient(IConfiguration configuration)
        {
            Configuration = configuration;
            Breadcrumbs = new Breadcrumbs(configuration);
            Delivery = new Delivery();
            Application.lowMemory += () => { _hasReceivedLowMemoryWarning = true; };
        }

        public void PopulateApp(App app)
        {
            AddIsLaunching(app);
            app.AddToPayload("lowMemory", _hasReceivedLowMemoryWarning);
        }

        private void AddIsLaunching(App app)
        {
            if (!app.ContainsKey("durationInForeground"))
            {
                return;
            }
            bool isLaunching;
            if (Configuration.LaunchDurationMillis == 0)
            {
                isLaunching = _launchMarkedAsCompleted;
            }
            else
            {
                isLaunching = app.DurationInForeground.Milliseconds < Configuration.LaunchDurationMillis;
            }
            app.IsLaunching = isLaunching;
        }

        public void PopulateDevice(Device device)
        {
            device.Manufacturer = "PC";
            device.Model =  SystemInfo.deviceModel;
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                device.FreeMemory = memStatus.ullAvailPhys;
            }

            // This is generally the main drive on a Windows machine
            // A future enhancement would be to determine which drive the application
            // is running on and use that drive letter instead of defaulting to C
            if (GetDiskFreeSpaceEx(@"C:\",
                                              out ulong freeBytesAvailable,
                                              out ulong totalNumberOfBytes,
                                              out ulong totalNumberOfFreeBytes))
            {
                device.FreeDisk = freeBytesAvailable;
            }
        }

        public void PopulateMetadata(Metadata metadata)
        {
        }

        public void PopulateUser(User user)
        {
        }

        public void SetMetadata(string tab, Dictionary<string, string> metadata)
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

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
      out ulong lpFreeBytesAvailable,
      out ulong lpTotalNumberOfBytes,
      out ulong lpTotalNumberOfFreeBytes);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(this);
            }
        }

        public LastRunInfo GetLastRunInfo()
        {
            return null;
        }
    }
}
