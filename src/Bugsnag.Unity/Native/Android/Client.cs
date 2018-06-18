using System.Collections.Generic;
using UnityEngine;

namespace Bugsnag.Unity.Native
{
  static class Client
  {
    static readonly AndroidJavaClass Bugsnag
      = new AndroidJavaClass("com.bugsnag.android.Bugsnag");

    const string NOTIFIER_NAME = "Bugsnag Unity (Android)";
    const string NOTIFIER_URL = "https://github.com/bugsnag/bugsnag-unity";

    internal static void Register(string apiKey, Dictionary<string, string> unityMetadata)
    {
      using (var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
      using (var configuration = new AndroidJavaObject("com.bugsnag.android.Configuration", apiKey))
      {
        var activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        var context = activity.Call<AndroidJavaObject>("getApplicationContext");
        // the bugsnag-unity notifier will handle session tracking
        configuration.Call("setAutoCaptureSessions", false);
        Bugsnag.CallStatic<AndroidJavaObject>("init", context, configuration);
      }

      foreach (var item in unityMetadata)
      {
        Bugsnag.CallStatic("addToTab", "Unity", item.Key, item.Value);
      }

      using (var notifier = new AndroidJavaClass("com.bugsnag.android.Notifier"))
      {
        var notifierInstance = notifier.CallStatic<AndroidJavaObject>("getInstance");
        // possibly we can set notifier type which seems to be what react native does?
        notifierInstance.Call("setName", NOTIFIER_NAME);
        notifierInstance.Call("setVersion", typeof(Client).Assembly.GetName().Version.ToString(3));
        notifierInstance.Call("setURL", NOTIFIER_URL);
      }
    }

    internal static void SetNotifyUrl(string notifyUrl)
    {
      Bugsnag.CallStatic("setEndpoint", notifyUrl);
    }

    internal static void SetAutoNotify(bool autoNotify)
    {
      if (autoNotify)
      {
        Bugsnag.CallStatic("enableExceptionHandler");
      }
      else
      {
        Bugsnag.CallStatic("disableExceptionHandler");
      }
    }

    internal static void SetContext(string context)
    {
      Bugsnag.CallStatic("setContext", context);
    }

    internal static void SetReleaseStage(string releaseStage)
    {
      Bugsnag.CallStatic("setReleaseStage", releaseStage);
    }

    internal static void SetNotifyReleaseStages(string[] releaseStages)
    {
      Bugsnag.CallStatic("setNotifyReleaseStages", releaseStages);
    }

    internal static void AddToTab(string tab, string key, string value)
    {
      Bugsnag.CallStatic("addToTab", tab, key, value);
    }

    internal static void ClearTab(string tabName)
    {
      Bugsnag.CallStatic("clearTab", tabName);
    }

    internal static void SetBreadcrumbCapacity(int capacity)
    {
      Bugsnag.CallStatic("setMaxBreadcrumbs", capacity);
    }

    internal static void SetAppVersion(string version)
    {
      Bugsnag.CallStatic("setAppVersion", version);
    }

    internal static void SetUser(string id, string name, string email)
    {
      Bugsnag.CallStatic("setUser", id, email, name);
    }
  }
}
