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
      using (var app = client.Call<AndroidJavaObject>("getAppData"))
      {
        this.AddToPayload("id", app.Call<string>("getPackageName"));
        this.AddToPayload("buildUUID", app.Call<string>("getBuildUuid"));
        this.AddToPayload("duration", app.Call<long>("getDuration"));
        this.AddToPayload("durationInForeground", app.Call<long>("getDurationInForeground"));
        this.AddToPayload("inForeground", app.Call<bool>("isInForeground"));
        this.AddToPayload("type", app.Call<string>("getNotifierType"));
        // release stage could probably doesn't need to be sourced from the android notifier?
        this.AddToPayload("releaseStage", app.Call<string>("getReleaseStage"));
        this.AddToPayload("version", app.Call<string>("getVersionName"));
        this.AddToPayload("codeBundleId", app.Call<string>("getCodeBundleId"));

        using (var versionCode = app.Call<AndroidJavaObject>("getVersionCode"))
        {
          if (versionCode != null)
          {
            this.AddToPayload("versionCode", versionCode.Call<int>("intValue"));
          }
        }
      }
    }
  }
}
