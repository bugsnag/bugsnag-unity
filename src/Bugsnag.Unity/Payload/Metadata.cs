using System.Collections.Generic;
using UnityEngine;

namespace Bugsnag.Unity.Payload
{
  public class Metadata : Dictionary<string, object>, IFilterable
  {
    public Metadata() : base()
    {
    }

    public Metadata(Metadata metadata) : base(metadata)
    {
    }
  }

  internal class AndroidMetadata : Metadata
  {
    internal AndroidMetadata(AndroidJavaObject client, Metadata metadata) : base(metadata)
    {
      using (var appData = client.Call<AndroidJavaObject>("getAppData"))
      using (var map = appData.Call<AndroidJavaObject>("getAppDataMetaData"))
      {
        var app = new Dictionary<string, object>();
        app.PopulateDictionaryFromAndroidData(map);
        this.AddToPayload("app", app);
      }

      using (var deviceData = client.Call<AndroidJavaObject>("getDeviceData"))
      using (var map = deviceData.Call<AndroidJavaObject>("getDeviceMetaData"))
      {
        var device = new Dictionary<string, object>();
        device.PopulateDictionaryFromAndroidData(map);
        this.AddToPayload("device", device);
      }
    }
  }
}
