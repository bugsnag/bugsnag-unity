using System.Collections.Generic;
using UnityEngine;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
  class AndroidBreadcrumbs : IBreadcrumbs
  {
    AndroidJavaObject Client { get; }

    internal AndroidBreadcrumbs(AndroidJavaObject client)
    {
      Client = client;
    }

    /// <summary>
    /// Add a breadcrumb to the collection using Manual type and no metadata.
    /// </summary>
    /// <param name="message"></param>
    public void Leave(string message)
    {
      Leave(message, BreadcrumbType.Manual, null);
    }

    /// <summary>
    /// Add a breadcrumb to the collection with the specified type and metadata
    /// </summary>
    /// <param name="message"></param>
    /// <param name="type"></param>
    /// <param name="metadata"></param>
    public void Leave(string message, BreadcrumbType type, IDictionary<string, string> metadata)
    {
      Leave(new Breadcrumb(message, type, metadata));
    }

    /// <summary>
    /// Add a pre assembled breadcrumb to the collection.
    /// </summary>
    /// <param name="breadcrumb"></param>
    public void Leave(Breadcrumb breadcrumb)
    {
      if (breadcrumb != null)
      {
        using (var metadata = new AndroidJavaObject("java.util.HashMap"))
        using (var type = new AndroidJavaClass("com.bugsnag.android.BreadcrumbType"))
        using (var breadcrumbType = type.GetStatic<AndroidJavaObject>(breadcrumb.Type.ToUpperInvariant()))
        {
          if (breadcrumb.Metadata != null)
          {
            var putMethod = AndroidJNIHelper.GetMethodID(metadata.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
            var args = new object[2];

            foreach (var item in breadcrumb.Metadata)
            {
              using (var key = new AndroidJavaObject("java.lang.String", item.Key))
              using (var value = new AndroidJavaObject("java.lang.String", item.Value))
              {
                args[0] = key;
                args[1] = value;
                AndroidJNI.CallObjectMethod(metadata.GetRawObject(), putMethod, AndroidJNIHelper.CreateJNIArgArray(args));
              }
            }
          }

          Client.Call("leaveBreadcrumb", breadcrumb.Name, breadcrumbType, metadata);
        }
      }
    }

    /// <summary>
    /// Retrieve the collection of breadcrumbs at this point in time.
    /// </summary>
    /// <returns></returns>
    public Breadcrumb[] Retrieve()
    {
      List<Breadcrumb> breadcrumbs = new List<Breadcrumb>();

      using (var javaBreadcrumbs = Client.Call<AndroidJavaObject>("getBreadcrumbs"))
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

      return breadcrumbs.ToArray();
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
  }
}
