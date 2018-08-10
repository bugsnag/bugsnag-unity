using System.Collections.Generic;
using System.Runtime.InteropServices;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
  class WindowsClient : INativeClient
  {
    public IConfiguration Configuration { get; }

    public IBreadcrumbs Breadcrumbs { get; }

    public IDelivery Delivery { get; }

    public WindowsClient(Configuration configuration)
    {
      Configuration = configuration;
      Breadcrumbs = new Breadcrumbs(configuration);
      Delivery = new Delivery();
    }

    public void PopulateApp(App app)
    {
    }

    public void PopulateDevice(Device device)
    {
      device.AddToPayload("jailbroken", false);

      MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
      if (GlobalMemoryStatusEx(memStatus))
      {
        device.AddToPayload("freeMemory", memStatus.ullAvailPhys);
      }

      if (GetDiskFreeSpaceEx(@"C:\",
                                        out ulong freeBytesAvailable,
                                        out ulong totalNumberOfBytes,
                                        out ulong totalNumberOfFreeBytes))
      {
        device.AddToPayload("freeDisk", freeBytesAvailable);
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

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
      out ulong lpFreeBytesAvailable,
      out ulong lpTotalNumberOfBytes,
      out ulong lpTotalNumberOfFreeBytes);

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

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
  }
}
