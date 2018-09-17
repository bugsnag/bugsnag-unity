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

    internal AndroidJavaObject JavaClient { get; }

    public NativeClient(Configuration configuration)
    {
      Configuration = configuration;

      using (var notifier = new AndroidJavaClass("com.bugsnag.android.Notifier"))
      using (var info = notifier.CallStatic<AndroidJavaObject>("getInstance"))
      {
        info.Call("setURL", NotifierInfo.NotifierUrl);
        info.Call("setName", NotifierInfo.NotifierName);
        info.Call("setVersion", NotifierInfo.NotifierVersion);
      }

      using (var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
      using (var activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"))
      using (var context = activity.Call<AndroidJavaObject>("getApplicationContext"))
      {
        JavaClient = new AndroidJavaObject("com.bugsnag.android.Client", context, configuration.JavaObject);

        // the bugsnag-android notifier uses Activity lifecycle tracking to
        // determine if the application is in the foreground. As the unity
        // activity has already started at this point we need to tell the
        // notifier about the activity and the fact that it has started.
        using (var sessionTracker = JavaClient.Get<AndroidJavaObject>("sessionTracker"))
        using (var activityClass = activity.Call<AndroidJavaObject>("getClass"))
        {
          var activityName = activityClass.CallStringMethod("getSimpleName");
          sessionTracker.Call("updateForegroundTracker", activityName, true, 0L);
        }
      }

      Delivery = new AndroidDelivery();
      Breadcrumbs = new Breadcrumbs(JavaClient);
    }

    public void PopulateApp(App app)
    {
      using (var appData = JavaClient.Call<AndroidJavaObject>("getAppData"))
      using (var map = appData.Call<AndroidJavaObject>("getAppData"))
      {
        app.PopulateDictionaryFromAndroidData(map);
      }
    }

    public void PopulateDevice(Device device)
    {
      using (var deviceData = JavaClient.Call<AndroidJavaObject>("getDeviceData"))
      using (var map = deviceData.Call<AndroidJavaObject>("getDeviceData"))
      {
        device.PopulateDictionaryFromAndroidData(map);
      }
    }

    public void PopulateUser(User user)
    {
      using (var nativeUser = JavaClient.Call<AndroidJavaObject>("getUser"))
      {
        user.Id = nativeUser.CallStringMethod("getId");
        user.Email = nativeUser.CallStringMethod("getEmail");
        user.Name = nativeUser.CallStringMethod("getName");
      }
    }

    public void SetMetadata(string tab, Dictionary<string, string> metadata)
    {
      using (var nativeMetadata = JavaClient.Call<AndroidJavaObject>("getMetaData"))
      {
        foreach (var item in metadata)
        {
          nativeMetadata.Call("addToTab", tab, item.Key, item.Value);
        }
      }
    }

    public void PopulateMetadata(Metadata metadata)
    {
      using (var appData = JavaClient.Call<AndroidJavaObject>("getAppData"))
      using (var map = appData.Call<AndroidJavaObject>("getAppDataMetaData"))
      {
        var app = new Dictionary<string, object>();
        app.PopulateDictionaryFromAndroidData(map);
        metadata.AddToPayload("app", app);
      }

      using (var deviceData = JavaClient.Call<AndroidJavaObject>("getDeviceData"))
      using (var map = deviceData.Call<AndroidJavaObject>("getDeviceMetaData"))
      {
        var device = new Dictionary<string, object>();
        device.PopulateDictionaryFromAndroidData(map);
        metadata.AddToPayload("device", device);
      }
    }
  }
}
