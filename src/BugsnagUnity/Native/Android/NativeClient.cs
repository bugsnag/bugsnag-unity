using System.Collections.Generic;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
  class NativeClient : INativeClient
  {
    public IConfiguration Configuration { get; }

    public IBreadcrumbs Breadcrumbs { get; }

    public IDelivery Delivery { get; }

    private NativeInterface NativeInterface;

    public NativeClient(IConfiguration configuration)
    {
      NativeInterface = new NativeInterface(configuration);
      Configuration = configuration;

      using (var notifier = new AndroidJavaClass("com.bugsnag.android.Notifier"))
      using (var info = notifier.CallStatic<AndroidJavaObject>("getInstance"))
      {
        info.Call("setURL", NotifierInfo.NotifierUrl);
        info.Call("setName", "Bugsnag Unity (Android)");
        info.Call("setVersion", NotifierInfo.NotifierVersion);
      }

      Delivery = new Delivery();
      Breadcrumbs = new Breadcrumbs(NativeInterface);
    }

    public void PopulateApp(App app)
    {
      MergeDictionaries(app, NativeInterface.GetAppData());
    }

    public void PopulateDevice(Device device)
    {
      Dictionary<string, object> runtimeVersions = (Dictionary<string, object>) device.Get("runtimeVersions");
      Dictionary<string, object> deviceData = NativeInterface.GetDeviceData();
      Dictionary<string, object> nativeVersions = (Dictionary<string, object>) deviceData.Get("runtimeVersions");

      deviceData.Remove("runtimeVersions"); // don't overwrite the unity version values
      MergeDictionaries(device, deviceData);
      MergeDictionaries(runtimeVersions, nativeVersions); // merge the native version values
    }

    public void PopulateUser(User user)
    {
      foreach(var entry in NativeInterface.GetUser()) {
        user.AddToPayload(entry.Key, entry.Value.ToString());
      }
    }

    public void SetMetadata(string tab, Dictionary<string, string> metadata)
    {
      if (metadata != null) {
        foreach (var item in metadata)
        {
          NativeInterface.AddToTab(tab, item.Key, item.Value);
        }
      } else {
        NativeInterface.RemoveMetadata(tab);
      }
    }

    public void PopulateMetadata(Metadata metadata)
    {
      MergeDictionaries(metadata, NativeInterface.GetMetaData());
    }

    private void MergeDictionaries(Dictionary<string, object> dest, Dictionary<string, object> another) {
      foreach(var entry in another) {
        dest.AddToPayload(entry.Key, entry.Value);
      }
    }

    public void SetSession(Session session)
    {
      NativeInterface.SetSession(session);
    }

    public void SetUser(User user)
    {
      NativeInterface.SetUser(user);
    }
  }

}
