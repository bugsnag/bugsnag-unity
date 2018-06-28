using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Bugsnag.Unity.Payload
{
  /// <summary>
  /// Represents the "device" key in the error report payload.
  /// </summary>
  class Device : Dictionary<string, object>, IFilterable
  {
    internal Device() : this(Hostname)
    {
    }

    internal Device(string hostname)
    {
      this.AddToPayload("hostname", hostname);
      this.AddToPayload("locale", CultureInfo.CurrentCulture.ToString());
      this.AddToPayload("timezone", TimeZone.CurrentTimeZone.StandardName);
      this.AddToPayload("osName", OsName);
      this.AddToPayload("time", DateTime.UtcNow);
    }

    /// <summary>
    /// Resolve the hostname using either "COMPUTERNAME" (win) or "HOSTNAME" (*nix) environment variable.
    /// </summary>
    private static string Hostname
    {
      get
      {
        return Environment.GetEnvironmentVariable("COMPUTERNAME") ?? Environment.GetEnvironmentVariable("HOSTNAME");
      }
    }

    private static string OsName
    {
      get
      {
        return Environment.OSVersion.VersionString;
      }
    }
  }

  class AndroidDevice : Device
  {
    internal AndroidDevice(AndroidJavaObject client)
    {
      using (var device = client.Call<AndroidJavaObject>("getDeviceData"))
      {
        this.AddToPayload("id", device.Call<string>("getId"));
        this.AddToPayload("freeMemory", device.Call<long>("getFreeMemory"));
        this.AddToPayload("totalMemory", device.Call<long>("getTotalMemory"));
        this.AddToPayload("orientation", device.Call<string>("getOrientation"));
        this.AddToPayload("manufacturer", device.Call<string>("getManufacturer"));
        this.AddToPayload("model", device.Call<string>("getModel"));
        this.AddToPayload("jailbroken", device.Call<bool>("isJailbroken"));
        this.AddToPayload("osName", device.Call<string>("getOsName"));
        this.AddToPayload("osVersion", device.Call<string>("getOsVersion"));

        using (var freeDisk = device.Call<AndroidJavaObject>("getFreeDisk"))
        {
          if (freeDisk != null)
          {
            this.AddToPayload("freeDisk", freeDisk.Call<long>("longValue"));
          }
        }
      }
    }
  }
}
