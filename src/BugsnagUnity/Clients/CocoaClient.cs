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

    ICocoaWrapper Wrapper { get; }

    public CocoaClient(MacOSConfiguration configuration)
    {
      Configuration = configuration;
      NativeConfiguration = configuration.NativeConfiguration;
      Wrapper = new MacOsWrapper();

      Wrapper.StartBugsnagWithConfiguration(configuration.NativeConfiguration);

      Delivery = new Delivery();
      Breadcrumbs = new MacOsBreadcrumbs(configuration);
    }

    public CocoaClient(iOSConfiguration configuration)
    {
      Configuration = configuration;
      NativeConfiguration = configuration.NativeConfiguration;
      Wrapper = new iOSWrapper();

      Wrapper.StartBugsnagWithConfiguration(configuration.NativeConfiguration);

      Delivery = new Delivery();
      Breadcrumbs = new iOSBreadcrumbs(configuration);
    }

    public void PopulateApp(App app)
    {
      GCHandle handle = GCHandle.Alloc(app);

      try
      {
        Wrapper.RetrieveAppData(GCHandle.ToIntPtr(handle), PopulateAppData);
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
        Wrapper.RetrieveDeviceData(GCHandle.ToIntPtr(handle), PopulateDeviceData);
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

    [StructLayout(LayoutKind.Sequential)]
    struct NativeUser
    {
      public IntPtr Id;
    }

    public void PopulateUser(User user)
    {
      var nativeUser = new NativeUser();

      Wrapper.PopulateUser(ref nativeUser);

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

      Wrapper.AddMetadata(NativeConfiguration, tab, metadata, metadata.Length);
    }

    public void PopulateMetadata(Metadata metadata)
    {
    }

    interface ICocoaWrapper
    {
      void StartBugsnagWithConfiguration(IntPtr configuration);

      void AddMetadata(IntPtr configuration, string tab, string[] metadata, int metadataCount);

      void RetrieveDeviceData(IntPtr instance, Action<IntPtr, string, string> populate);

      void RetrieveAppData(IntPtr instance, Action<IntPtr, string, string> populate);

      void PopulateUser(ref NativeUser user);
    }

    class MacOsWrapper : ICocoaWrapper
    {
      void ICocoaWrapper.StartBugsnagWithConfiguration(IntPtr configuration) => StartBugsnagWithConfiguration(configuration);
      void ICocoaWrapper.AddMetadata(IntPtr configuration, string tab, string[] metadata, int metadataCount) => AddMetadata(configuration, tab, metadata, metadataCount);
      void ICocoaWrapper.RetrieveDeviceData(IntPtr instance, Action<IntPtr, string, string> populate) => RetrieveDeviceData(instance, populate);
      void ICocoaWrapper.RetrieveAppData(IntPtr instance, Action<IntPtr, string, string> populate) => RetrieveAppData(instance, populate);
      void ICocoaWrapper.PopulateUser(ref NativeUser user) => PopulateUser(ref user);

      [DllImport("bugsnag-osx", EntryPoint = "bugsnag_startBugsnagWithConfiguration")]
      static extern void StartBugsnagWithConfiguration(IntPtr configuration);

      [DllImport("bugsnag-osx", EntryPoint = "bugsnag_setMetadata")]
      static extern void AddMetadata(IntPtr configuration, string tab, string[] metadata, int metadataCount);

      [DllImport("bugsnag-osx", EntryPoint = "bugsnag_retrieveDeviceData")]
      static extern void RetrieveDeviceData(IntPtr instance, Action<IntPtr, string, string> populate);

      [DllImport("bugsnag-osx", EntryPoint = "bugsnag_retrieveAppData")]
      static extern void RetrieveAppData(IntPtr instance, Action<IntPtr, string, string> populate);

      [DllImport("bugsnag-osx", EntryPoint = "bugsnag_populateUser")]
      static extern void PopulateUser(ref NativeUser user);
    }

    class iOSWrapper : ICocoaWrapper
    {
      void ICocoaWrapper.StartBugsnagWithConfiguration(IntPtr configuration) => StartBugsnagWithConfiguration(configuration);
      void ICocoaWrapper.AddMetadata(IntPtr configuration, string tab, string[] metadata, int metadataCount) => AddMetadata(configuration, tab, metadata, metadataCount);
      void ICocoaWrapper.RetrieveDeviceData(IntPtr instance, Action<IntPtr, string, string> populate) => RetrieveDeviceData(instance, populate);
      void ICocoaWrapper.RetrieveAppData(IntPtr instance, Action<IntPtr, string, string> populate) => RetrieveAppData(instance, populate);
      void ICocoaWrapper.PopulateUser(ref NativeUser user) => PopulateUser(ref user);

      [DllImport("__Internal", EntryPoint = "bugsnag_startBugsnagWithConfiguration")]
      static extern void StartBugsnagWithConfiguration(IntPtr configuration);

      [DllImport("__Internal", EntryPoint = "bugsnag_setMetadata")]
      static extern void AddMetadata(IntPtr configuration, string tab, string[] metadata, int metadataCount);

      [DllImport("__Internal", EntryPoint = "bugsnag_retrieveAppData")]
      static extern void RetrieveAppData(IntPtr instance, Action<IntPtr, string, string> populate);

      [DllImport("__Internal", EntryPoint = "bugsnag_retrieveDeviceData")]
      static extern void RetrieveDeviceData(IntPtr instance, Action<IntPtr, string, string> populate);

      [DllImport("__Internal", EntryPoint = "bugsnag_populateUser")]
      static extern void PopulateUser(ref NativeUser user);
    }
  }
}
