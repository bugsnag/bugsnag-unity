using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace Bugsnag.Unity.Payload
{
  /// <summary>
  /// Represents the "app" key in the error report payload.
  /// </summary>
  class App : Dictionary<string, object>, IFilterable
  {
    internal App(IConfiguration configuration) : this(configuration.AppVersion, configuration.ReleaseStage, null)
    {

    }

    internal App(string version, string releaseStage, string type)
    {
      this.AddToPayload("version", version);
      this.AddToPayload("releaseStage", releaseStage);
      this.AddToPayload("type", type);
    }
  }

  class AndroidApp : App
  {
    internal AndroidApp(IConfiguration configuration, AndroidJavaObject client) : base(configuration)
    {
      using (var appData = client.Call<AndroidJavaObject>("getAppData"))
      using (var map = appData.Call<AndroidJavaObject>("getAppData"))
      {
        this.PopulateDictionaryFromAndroidData(map);
      }
    }
  }

  class MacOsApp : App
  {
    [DllImport("bugsnag-osx", EntryPoint = "retrieveAppData")]
    static extern void RetrieveAppData(IntPtr instance, Action<IntPtr, string, string> populate);

    internal MacOsApp(IConfiguration configuration) : base(configuration)
    {
      GCHandle handle;

      try
      {
        handle = GCHandle.Alloc(this);

        RetrieveAppData(GCHandle.ToIntPtr(handle), PopulateAppData);
      }
      finally
      {
        handle.Free();
      }
    }

    [MonoPInvokeCallback(typeof(Action<IntPtr, string, string>))]
    static void PopulateAppData(IntPtr instance, string key, string value)
    {
      var handle = GCHandle.FromIntPtr(instance);
      if (handle.Target is MacOsApp app)
      {
        app.AddToPayload(key, value);
      }
    }
  }

  class iOSApp : App
  {
    [DllImport("__Internal", EntryPoint = "retrieveAppData")]
    static extern void RetrieveAppData(IntPtr instance, Action<IntPtr, string, string> populate);

    internal iOSApp(IConfiguration configuration) : base(configuration)
    {
      GCHandle handle;

      try
      {
        handle = GCHandle.Alloc(this);

        RetrieveAppData(GCHandle.ToIntPtr(handle), PopulateAppData);
      }
      finally
      {
        handle.Free();
      }
    }

    [MonoPInvokeCallback(typeof(Action<IntPtr, string, string>))]
    static void PopulateAppData(IntPtr instance, string key, string value)
    {
      var handle = GCHandle.FromIntPtr(instance);
      if (handle.Target is iOSApp app)
      {
        app.AddToPayload(key, value);
      }
    }
  }
}
