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

    public NativeClient(Configuration configuration)
    {
      NativeInterface = configuration.NativeInterface;
      Configuration = configuration;

      using (var notifier = new AndroidJavaClass("com.bugsnag.android.Notifier"))
      using (var info = notifier.CallStatic<AndroidJavaObject>("getInstance"))
      {
        info.Call("setURL", NotifierInfo.NotifierUrl);
        info.Call("setName", NotifierInfo.NotifierName);
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
      MergeDictionaries(device, NativeInterface.GetDeviceData());
    }

    public void PopulateUser(User user)
    {
      foreach(var entry in NativeInterface.GetUser()) {
        user.AddToPayload(entry.Key, entry.Value.ToString());
      }
    }

    public void SetMetadata(string tab, Dictionary<string, string> metadata)
    {
      foreach (var item in metadata)
      {
        NativeInterface.AddToTab(tab, item.Key, item.Value);
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

  }

}
