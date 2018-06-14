using Bugsnag.Unity.Payload;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bugsnag.Unity.Native
{
  public class Breadcrumbs
  {
    static readonly AndroidJavaClass Bugsnag
      = new AndroidJavaClass("com.bugsnag.android.Bugsnag");
     
    internal Breadcrumbs(Configuration configuration)
    {

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
      using (var metadata = new AndroidJavaObject("java.util.HashMap"))
      using (var type = new AndroidJavaClass("com.bugsnag.android.BreadcrumbType"))
      {
        foreach (var item in breadcrumb.Metadata)
        {
          metadata.Call<string>("put", item.Key, item.Value);
        }

        Bugsnag.CallStatic("leaveBreadcrumb", breadcrumb.Name, type.GetStatic<AndroidJavaObject>(breadcrumb.Type.ToUpperInvariant()), metadata);
      }
    }

    /// <summary>
    /// Retrieve the collection of breadcrumbs at this point in time.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Breadcrumb> Retrieve()
    {
      var client = Bugsnag.CallStatic<AndroidJavaObject>("getClient");
      var breadcrumbs = client.Call<AndroidJavaObject[]>("getBreadcrumbs");

      return breadcrumbs.Select(ConvertToBreadcrumb).ToArray();
    }

    static Breadcrumb ConvertToBreadcrumb(AndroidJavaObject javaBreadcrumb)
    {
      var metadata = new Dictionary<string, string>();

      var javaMetadata = javaBreadcrumb.Call<AndroidJavaObject>("getMetadata");
      var set = javaMetadata.Call<AndroidJavaObject>("entrySet");
      var iterator = set.Call<AndroidJavaObject>("iterator");

      while (iterator.Call<bool>("hasNext"))
      {
        var next = iterator.Call<AndroidJavaObject>("next");
        metadata.Add(next.Call<string>("getKey"), next.Call<string>("getValue"));
      }

      var name = javaBreadcrumb.Call<string>("getName");
      var type = javaBreadcrumb.Call<AndroidJavaObject>("getType").Call<string>("toString");
      // accessing the timestamp is not possible right now
      var timestamp = javaBreadcrumb.Call<string>("getTimestamp");
      //var timestamp = DateTime.UtcNow.ToUniversalTime().ToString(@"yyyy-MM-dd\THH:mm:ss.FFFFFFF\Z");

      return new Breadcrumb(name, timestamp, type, metadata);
    }
  }
}
