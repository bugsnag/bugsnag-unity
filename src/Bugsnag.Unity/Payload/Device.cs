using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using AOT;
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
      {
        this.PopulateDictionaryFromAndroidData(map);
      }
    }
  }

  class MacOsDevice : Device
  {
    [DllImport("bugsnag-osx", EntryPoint = "retrieveDeviceData")]
    static extern void RetrieveAppData(IntPtr instance, Action<IntPtr, string, string> populate);
    
    internal MacOsDevice()
    {
      var handle = GCHandle.Alloc(this);

      RetrieveAppData(GCHandle.ToIntPtr(handle), PopulateDeviceData);

      handle.Free();
    }

    [MonoPInvokeCallback(typeof(Action<IntPtr, string, string>))]
    static void PopulateDeviceData(IntPtr instance, string key, string value)
    {
      var handle = GCHandle.FromIntPtr(instance);
      if (handle.Target is MacOsDevice app)
      {
        app.AddToPayload(key, value);
      }
    }
  }

  class iOSDevice : Device
  {
    [DllImport("__Internal", EntryPoint = "retrieveDeviceData")]
    static extern void RetrieveAppData(IntPtr instance, Action<IntPtr, string, string> populate);

    internal iOSDevice()
    {
      var handle = GCHandle.Alloc(this);

      RetrieveAppData(GCHandle.ToIntPtr(handle), PopulateDeviceData);

      handle.Free();
    }

    [MonoPInvokeCallback(typeof(Action<IntPtr, string, string>))]
    static void PopulateDeviceData(IntPtr instance, string key, string value)
    {
      var handle = GCHandle.FromIntPtr(instance);
      if (handle.Target is iOSDevice app)
      {
        app.AddToPayload(key, value);
      }
    }
  }
}
