using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
  class iOSClient : INativeClient
  {
    [DllImport("__Internal", EntryPoint = "bugsnag_startBugsnagWithConfiguration")]
    static extern void StartBugsnagWithConfiguration(IntPtr configuration);

    [DllImport("__Internal", EntryPoint = "bugsnag_setMetadata")]
    static extern void AddMetadata(IntPtr configuration, string tab, string[] metadata, int metadataCount);

    [DllImport("__Internal", EntryPoint = "bugsnag_retrieveAppData")]
    static extern void RetrieveAppData(IntPtr instance, Action<IntPtr, string, string> populate);

    [DllImport("__Internal", EntryPoint = "bugsnag_retrieveDeviceData")]
    static extern void RetrieveDeviceData(IntPtr instance, Action<IntPtr, string, string> populate);

    iOSConfiguration Configuration { get; }

    IConfiguration INativeClient.Configuration => Configuration;

    public IBreadcrumbs Breadcrumbs { get; }

    public IDelivery Delivery { get; }

    public iOSClient(iOSConfiguration configuration)
    {
      Configuration = configuration;
      StartBugsnagWithConfiguration(configuration.NativeConfiguration);

      Delivery = new Delivery();
      Breadcrumbs = new iOSBreadcrumbs(configuration);
    }

    public void PopulateApp(App app)
    {
      GCHandle handle = GCHandle.Alloc(app);

      try
      {
        RetrieveAppData(GCHandle.ToIntPtr(handle), PopulateAppData);
      }
      finally
      {
        if (handle != null)
        {
          handle.Free();
        }
      }
    }

    [MonoPInvokeCallback(typeof(Action<IntPtr, string, string>))]
    static void PopulateAppData(IntPtr instance, string key, string value)
    {
      var handle = GCHandle.FromIntPtr(instance);
      if (handle.Target is App app)
      {
        app.AddToPayload(key, value);
      }
    }

    public void PopulateDevice(Device device)
    {
      var handle = GCHandle.Alloc(device);

      try
      {
        RetrieveDeviceData(GCHandle.ToIntPtr(handle), PopulateDeviceData);
      }
      finally
      {
        handle.Free();
      }
    }

    [MonoPInvokeCallback(typeof(Action<IntPtr, string, string>))]
    static void PopulateDeviceData(IntPtr instance, string key, string value)
    {
      var handle = GCHandle.FromIntPtr(instance);
      if (handle.Target is Device device)
      {
        device.AddToPayload(key, value);
      }
    }

    public void PopulateUser(User user)
    {
    }

    public void SetMetadata(string tab, Dictionary<string, string> unityMetadata)
    {
      var index = 0;
      var metadata = new string[unityMetadata.Count * 2];

      foreach (var data in unityMetadata)
      {
        metadata[index] = data.Key;
        metadata[index + 1] = data.Value;
        index += 2;
      }

      AddMetadata(Configuration.NativeConfiguration, tab, metadata, metadata.Length);
    }

    public void PopulateMetadata(Metadata metadata)
    {
    }
  }
}
