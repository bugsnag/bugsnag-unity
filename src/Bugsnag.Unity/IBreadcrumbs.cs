using System.Collections.Generic;
using UnityEngine;
using Bugsnag.Unity.Payload;

namespace Bugsnag.Unity
{
  public interface IBreadcrumbs
  {
    void Leave(string message);

    void Leave(string message, BreadcrumbType type, IDictionary<string, string> metadata);

    void Leave(Breadcrumb breadcrumb);

    Breadcrumb[] Retrieve();
  }

  class Breadcrumbs : IBreadcrumbs
  {
    private readonly object _lock = new object();
    private readonly int _maximumBreadcrumbs;
    private readonly Breadcrumb[] _breadcrumbs;
    private int _current;

    /// <summary>
    /// Constructs a collection of breadcrumbs
    /// </summary>
    /// <param name="configuration"></param>
    internal Breadcrumbs(IConfiguration configuration)
    {
      _maximumBreadcrumbs = configuration.MaximumBreadcrumbs;
      _current = 0;
      _breadcrumbs = new Breadcrumb[_maximumBreadcrumbs];
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
        lock (_lock)
        {
          _breadcrumbs[_current] = breadcrumb;
          _current = (_current + 1) % _maximumBreadcrumbs;
        }
      }
    }

    /// <summary>
    /// Retrieve the collection of breadcrumbs at this point in time.
    /// </summary>
    /// <returns></returns>
    public Breadcrumb[] Retrieve()
    {
      //var x = Native.Client.GetBreadcrumbs();
      //return x.Select(b => new Breadcrumb(b));
      lock (_lock)
      {
        var numberOfBreadcrumbs = System.Array.IndexOf(_breadcrumbs, null);

        if (numberOfBreadcrumbs < 0) numberOfBreadcrumbs = _maximumBreadcrumbs;

        var breadcrumbs = new Breadcrumb[numberOfBreadcrumbs];

        for (int i = 0; i < numberOfBreadcrumbs; i++)
        {
          breadcrumbs[i] = _breadcrumbs[i];
        }

        return breadcrumbs;
      }
    }
  }

  class AndroidBreadcrumbs : IBreadcrumbs
  {
    AndroidJavaObject Client { get; }

    internal AndroidBreadcrumbs(AndroidClient client)
    {
      Client = client.JavaObject;
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
      using (var breadcrumbType = type.GetStatic<AndroidJavaObject>(breadcrumb.Type.ToUpperInvariant()))
      {
        foreach (var item in breadcrumb.Metadata)
        {
          metadata.Call<string>("put", item.Key, item.Value);
        }

        Client.Call("leaveBreadcrumb", breadcrumb.Name, breadcrumbType, metadata);
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
            metadata.Add(next.Call<string>("getKey"), next.Call<string>("getValue"));
          }
        }
      }

      using (var type = javaBreadcrumb.Call<AndroidJavaObject>("getType"))
      {
        var name = javaBreadcrumb.Call<string>("getName");
        // accessing the timestamp is not possible right now
        var timestamp = javaBreadcrumb.Call<string>("getTimestamp");
        //var timestamp = DateTime.UtcNow.ToUniversalTime().ToString(@"yyyy-MM-dd\THH:mm:ss.FFFFFFF\Z");

        return new Breadcrumb(name, timestamp, type.Call<string>("toString"), metadata);
      }
    }
  }
}
