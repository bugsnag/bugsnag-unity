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
      var appMetadata = new Dictionary<string, string>();

      using (var appData = client.Call<AndroidJavaObject>("getAppData"))
      using (var map = appData.Call<AndroidJavaObject>("getAppDataMetaData"))
      {
        this.AddToPayload("app", CreateDictionaryFromJavaMap(map));
      }

      this.AddToPayload("app", appMetadata);

      using (var deviceData = client.Call<AndroidJavaObject>("getDeviceData"))
      using (var map = deviceData.Call<AndroidJavaObject>("getDeviceMetaData"))
      {
        this.AddToPayload("device", CreateDictionaryFromJavaMap(map));
      }
    }

    private Dictionary<string, string> CreateDictionaryFromJavaMap(AndroidJavaObject map)
    {
      var dictionary = new Dictionary<string, string>();

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
              dictionary.Add(key, value.Call<string>("toString"));
            }
          }
        }
      }

      return dictionary;
    }
  }
}
