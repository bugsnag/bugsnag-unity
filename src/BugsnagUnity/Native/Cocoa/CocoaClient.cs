using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
  class CocoaClient : INativeClient
  {
    public IConfiguration Configuration { get; }

    public IBreadcrumbs Breadcrumbs { get; }

    public IDelivery Delivery { get; }

    IntPtr NativeConfiguration { get; }
    
    CocoaClient(IConfiguration configuration, IntPtr nativeConfiguration, IBreadcrumbs breadcrumbs)
    {
      Configuration = configuration;
      NativeConfiguration = nativeConfiguration;

      NativeCode.StartBugsnagWithConfiguration(NativeConfiguration);

      Delivery = new Delivery();
      Breadcrumbs = breadcrumbs;
    }

    public CocoaClient(Configuration configuration) : this(configuration, configuration.NativeConfiguration, new Breadcrumbs(configuration))
    {
    }

    public void PopulateApp(App app)
    {
      GCHandle handle = GCHandle.Alloc(app);

      try
      {
        NativeCode.RetrieveAppData(GCHandle.ToIntPtr(handle), PopulateAppData);
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
        NativeCode.RetrieveDeviceData(GCHandle.ToIntPtr(handle), PopulateDeviceData);
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
        switch (key) {
          case "jailbroken":
            switch (value) {
              case "0":
                device.AddToPayload(key, false);
                break;
              case "1":
                device.AddToPayload(key, true);
                break;
              default:
                device.AddToPayload(key, value);
                break;
            }
            break;
          default:
            device.AddToPayload(key, value);
            break;
        }
      }
    }

    public void PopulateUser(User user)
    {
      var nativeUser = new NativeUser();

      NativeCode.PopulateUser(ref nativeUser);

      user.Id = Marshal.PtrToStringAuto(nativeUser.Id);
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

      NativeCode.AddMetadata(NativeConfiguration, tab, metadata, metadata.Length);
    }

    public void PopulateMetadata(Metadata metadata)
    {
    }
  }
}
