using System.Collections.Generic;
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
              this.AddToPayload(key, value.Call<string>("toString"));
            }
          }
        }
      }
    }
  }
}
