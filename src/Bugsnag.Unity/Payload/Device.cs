using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
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
      using (var deviceData = client.Call<AndroidJavaObject>("getDeviceData"))
      using (var map = deviceData.Call<AndroidJavaObject>("getDeviceData"))
      using (var set = map.Call<AndroidJavaObject>("entrySet"))
      using (var iterator = set.Call<AndroidJavaObject>("iterator"))
      {
        while (iterator.Call<bool>("hasNext"))
        {
          using (var mapEntry = iterator.Call<AndroidJavaObject>("next"))
          {
            var key = mapEntry.Call<string>("getKey");
            var value = mapEntry.Call<AndroidJavaObject>("getValue");

            if (value != null)
            {
              // how can we handle classes that don't return useful things
              // from toString?
              this.AddToPayload(key, value.Call<string>("toString"));
            }
          }
        }
      }
    }
  }

  class MacOsDevice : Device
  {
    [DllImport("bugsnag-osx", EntryPoint = "retrieveDeviceData")]
    static extern void RetrieveAppData(Action<string, string> populate);
    
    internal MacOsDevice()
    {
      RetrieveAppData((key, value) => this.AddToPayload(key, value));      
    }
  }
}
