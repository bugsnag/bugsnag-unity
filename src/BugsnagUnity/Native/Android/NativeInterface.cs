using System;
using System.Collections.Generic;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
  class NativeInterface
  {
    public NativeInterface(Configuration configuration)
    {
      using (var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
      using (var activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"))
      using (var context = activity.Call<AndroidJavaObject>("getApplicationContext"))
      {
        // lookup the NativeInterface class and set the client to the local object.
        // all subsequent communication should go through the NativeInterface.
        var client = new AndroidJavaObject("com.bugsnag.android.Client", context, configuration.JavaObject);
        var nativeInterfaceRef = AndroidJNI.FindClass("com/bugsnag/android/NativeInterface");
        var BugsnagNativeInterface = AndroidJNI.NewGlobalRef(nativeInterfaceRef);
        AndroidJNI.DeleteLocalRef(nativeInterfaceRef);
        var setClient = AndroidJNI.GetStaticMethodID(BugsnagNativeInterface, "setClient", "(Lcom/bugsnag/android/Client;)V");

        jvalue[] args = AndroidJNIHelper.CreateJNIArgArray(new object[] {client});
        AndroidJNI.CallStaticVoidMethod(BugsnagNativeInterface, setClient, args);

        // the bugsnag-android notifier uses Activity lifecycle tracking to
        // determine if the application is in the foreground. As the unity
        // activity has already started at this point we need to tell the
        // notifier about the activity and the fact that it has started.
        using (var sessionTracker = client.Get<AndroidJavaObject>("sessionTracker"))
        using (var activityClass = activity.Call<AndroidJavaObject>("getClass"))
        {
          var activityName = activityClass.CallStringMethod("getSimpleName");
          sessionTracker.Call("updateForegroundTracker", activityName, true, 0L);
        }
      }
    }
    public Dictionary<string, object> GetAppData() {
      return GetJavaMapData("getAppData");
    }

    public Dictionary<string, object> GetDeviceData() {
      return GetJavaMapData("getDeviceData");
    }

    public Dictionary<string, object> GetMetaData() {
      return GetJavaMapData("getMetaData");
    }

    public Dictionary<string, object> GetUser() {
      return GetJavaMapData("getUserData");
    }

    public void AddToTab(string tab, string key, string value) {
      using (var nativeInterface = new AndroidJavaClass("com.bugsnag.android.NativeInterface"))
      {
        nativeInterface.CallStatic("addToTab", tab, key, value);
      }
    }

    public void LeaveBreadcrumb(string name, string type, IDictionary<string, string> metadata) {
      using (var nativeInterface = new AndroidJavaClass("com.bugsnag.android.NativeInterface"))
      using (var map = JavaMapFromDictionary(metadata))
      {
        nativeInterface.CallStatic("leaveBreadcrumb", name, type, map);
      }
    }

    public List<Breadcrumb> GetBreadcrumbs()
    {
      List<Breadcrumb> breadcrumbs = new List<Breadcrumb>();

      using (var nativeInterface = new AndroidJavaClass("com.bugsnag.android.NativeInterface"))
      using (var javaBreadcrumbs = nativeInterface.CallStatic<AndroidJavaObject>("getBreadcrumbs"))
      using (var iterator = javaBreadcrumbs.Call<AndroidJavaObject>("iterator"))
      {
        while (iterator.Call<bool>("hasNext"))
        {
          using (var next = iterator.Call<AndroidJavaObject>("next"))
          {
            breadcrumbs.Add(ConvertToBreadcrumb(next));
          }
        }
      }
      return breadcrumbs;
    }

    static Breadcrumb ConvertToBreadcrumb(AndroidJavaObject javaBreadcrumb)
    {
      var metadata = new Dictionary<string, string>();

      using (var javaMetadata = javaBreadcrumb.Call<AndroidJavaObject>("getMetadata"))
      using (var set = javaMetadata.Call<AndroidJavaObject>("entrySet"))
      using (var iterator = set.Call<AndroidJavaObject>("iterator"))
      {
        while (iterator.Call<bool>("hasNext"))
        {
          using (var next = iterator.Call<AndroidJavaObject>("next"))
          {
            metadata.Add(next.CallStringMethod("getKey"), next.CallStringMethod("getValue"));
          }
        }
      }

      using (var type = javaBreadcrumb.Call<AndroidJavaObject>("getType"))
      {
        var name = javaBreadcrumb.CallStringMethod("getName");
        var timestamp = javaBreadcrumb.CallStringMethod("getTimestamp");

        return new Breadcrumb(name, timestamp, type.CallStringMethod("toString"), metadata);
      }
    }

    private Dictionary<string, object> GetJavaMapData(string methodName) {
      using (var nativeInterface = new AndroidJavaClass("com.bugsnag.android.NativeInterface"))
      using (var data = nativeInterface.CallStatic<AndroidJavaObject>(methodName))
      {
        return DictionaryFromJavaMap(data);
      }
    }

    private AndroidJavaObject JavaMapFromDictionary(IDictionary<string, string> src) {
      var map = new AndroidJavaObject("java.util.HashMap");
      if (src != null)
      {
        foreach(var entry in src)
        {
          map.Call<AndroidJavaObject>("put", entry.Key, entry.Value);
        }
      }
      return map;
    }

    private Dictionary<string, object> DictionaryFromJavaMap(AndroidJavaObject source) {
      var dict = new Dictionary<string, object>();

      using (var set = source.Call<AndroidJavaObject>("entrySet"))
      using (var iterator = set.Call<AndroidJavaObject>("iterator"))
      {
        var mapClz = AndroidJNI.FindClass("java/util/Map");

        while (iterator.Call<bool>("hasNext"))
        {
          using (var mapEntry = iterator.Call<AndroidJavaObject>("next"))
          {
            var key = mapEntry.CallStringMethod("getKey");
            using (var value = mapEntry.Call<AndroidJavaObject>("getValue"))
            {
              if (value != null)
              {
                var objRef = value.GetRawObject();

                using (var @class = value.Call<AndroidJavaObject>("getClass"))
                {
                  if (@class.Call<bool>("isArray"))
                  {
                    var values = AndroidJNIHelper.ConvertFromJNIArray<string[]>(value.GetRawObject());
                    dict.AddToPayload(key, values);
                  }
                  else if (AndroidJNI.IsInstanceOf(objRef, mapClz))
                  {
                    dict.AddToPayload(key, DictionaryFromJavaMap(value));
                  }
                  else
                  {
                    dict.AddToPayload(key, value.CallStringMethod("toString"));
                  }
                }
              }
            }
          }
        }
      }
      return dict;
    }

  }

}
