using System.Collections.Generic;
using UnityEngine;

namespace Bugsnag.Native
{
  public static class Client
  {
    private static readonly AndroidJavaClass Bugsnag
      = new AndroidJavaClass("com.bugsnag.android.Bugsnag");

    private const string NOTIFIER_NAME = "Bugsnag Unity (Android)";
    private const string NOTIFIER_URL = "https://github.com/bugsnag/bugsnag-unity";

    public static void Register(string apiKey, Dictionary<string, string> unityMetadata)
    {
      Register(apiKey, false, unityMetadata);
    }

    public static void Register(string apiKey, bool trackSessions, Dictionary<string, string> unityMetadata)
    {
      var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
      var activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
      var context = activity.Call<AndroidJavaObject>("getApplicationContext");

      var configuration = new AndroidJavaObject("com.bugsnag.android.Configuration", apiKey);
      configuration.Call("setAutoCaptureSessions", trackSessions);

      Bugsnag.CallStatic<AndroidJavaObject>("init", context, configuration);

      foreach (var item in unityMetadata)
      {
        Bugsnag.CallStatic("addToTab", "Unity", item.Key, item.Value);
      }

      var notifier = new AndroidJavaClass("com.bugsnag.android.Notifier");
      var notifierInstance = notifier.CallStatic<AndroidJavaObject>("getInstance");
      // possibly we can set notifier type which seems to be what react native does?
      notifierInstance.Call("setName", NOTIFIER_NAME);
      notifierInstance.Call("setVersion", typeof(Client).Assembly.GetName().Version.ToString(3));
      notifierInstance.Call("setURL", NOTIFIER_URL);
    }

    public static void SetNotifyUrl(string notifyUrl)
    {
      Bugsnag.CallStatic("setEndpoint", notifyUrl);
    }

    public static void SetAutoNotify(bool autoNotify)
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

    public static void SetContext(string context)
    {
      Bugsnag.CallStatic("setContext", context);
    }

    public static void SetReleaseStage(string releaseStage)
    {
      Bugsnag.CallStatic("setReleaseStage", releaseStage);
    }

    public static void SetNotifyReleaseStages(string[] releaseStages)
    {
      Bugsnag.CallStatic("setNotifyReleaseStages", releaseStages);
    }

    public static void AddToTab(string tab, string key, string value)
    {
      Bugsnag.CallStatic("addToTab", tab, key, value);
    }

    public static void ClearTab(string tabName)
    {
      Bugsnag.CallStatic("clearTab", tabName);
    }

    public static void LeaveBreadcrumb(string breadcrumb)
    {
      Bugsnag.CallStatic("leaveBreadcrumb", breadcrumb);
    }

    public static void SetBreadcrumbCapacity(int capacity)
    {
      Bugsnag.CallStatic("setMaxBreadcrumbs", capacity);
    }

    public static void SetAppVersion(string version)
    {
      Bugsnag.CallStatic("setAppVersion", version);
    }

    public static void SetUser(string id, string name, string email)
    {
      Bugsnag.CallStatic("setUser", id, email, name);
    }
  }
}
