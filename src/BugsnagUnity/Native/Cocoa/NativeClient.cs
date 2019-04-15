using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
  class NativeClient : INativeClient
  {
    public IConfiguration Configuration { get; }

    public IBreadcrumbs Breadcrumbs { get; }

    public IDelivery Delivery { get; }

    IntPtr NativeConfiguration { get; }
    
    NativeClient(IConfiguration configuration, IntPtr nativeConfiguration, IBreadcrumbs breadcrumbs)
    {
      Configuration = configuration;
      NativeConfiguration = nativeConfiguration;

      NativeCode.bugsnag_startBugsnagWithConfiguration(NativeConfiguration, NotifierInfo.NotifierVersion);

      Delivery = new Delivery();
      Breadcrumbs = breadcrumbs;
    }

    public NativeClient(Configuration configuration) : this(configuration, configuration.NativeConfiguration, new Breadcrumbs(configuration))
    {
    }

    public void PopulateApp(App app)
    {
      GCHandle handle = GCHandle.Alloc(app);

      try
      {
        NativeCode.bugsnag_retrieveAppData(GCHandle.ToIntPtr(handle), PopulateAppData);
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
        NativeCode.bugsnag_retrieveDeviceData(GCHandle.ToIntPtr(handle), PopulateDeviceData);
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

      NativeCode.bugsnag_populateUser(ref nativeUser);

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

      NativeCode.bugsnag_setMetadata(NativeConfiguration, tab, metadata, metadata.Length);
    }

    public void PopulateMetadata(Metadata metadata)
    {
    }

    public void SetSession(Session session)
    {
      if (session == null)
      {
        // Clear session
        NativeCode.bugsnag_registerSession(null, 0, 0, 0);
      }
      else
      {
        // The ancient version of the runtime used doesn't have an equivalent to GetUnixTime()
        var startedAt = (session.StartedAt - new DateTime(1970, 1, 1, 0, 0, 0, 0)).Milliseconds;
        NativeCode.bugsnag_registerSession(session.Id.ToString(), startedAt, session.UnhandledCount(), session.HandledCount());
      }
    }

    public void SetUser(User user)
    {
      NativeCode.bugsnag_setUser(user.Id, user.Name, user.Email);
    }
  }
}
