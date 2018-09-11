using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using BugsnagUnity.Payload;

namespace BugsnagUnity
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
    private IConfiguration Configuration { get; }
    private Breadcrumb[] _breadcrumbs;
    private int _current;

    /// <summary>
    /// Constructs a collection of breadcrumbs
    /// </summary>
    /// <param name="configuration"></param>
    internal Breadcrumbs(IConfiguration configuration)
    {
      Configuration = configuration;
      _current = 0;
      _breadcrumbs = new Breadcrumb[Configuration.MaximumBreadcrumbs];
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
          var maximumBreadcrumbs = Configuration.MaximumBreadcrumbs;
          if (_breadcrumbs.Length != maximumBreadcrumbs)
          {
            Array.Resize(ref _breadcrumbs, maximumBreadcrumbs);
            if (_current >= maximumBreadcrumbs)
            {
              _current = 0;
            }
          }
          _breadcrumbs[_current] = breadcrumb;
          _current = (_current + 1) % maximumBreadcrumbs;
        }
      }
    }

    /// <summary>
    /// Retrieve the collection of breadcrumbs at this point in time.
    /// </summary>
    /// <returns></returns>
    public Breadcrumb[] Retrieve()
    {
      lock (_lock)
      {
        var numberOfBreadcrumbs = Array.IndexOf(_breadcrumbs, null);

        if (numberOfBreadcrumbs < 0) numberOfBreadcrumbs = _breadcrumbs.Length;

        var breadcrumbs = new Breadcrumb[numberOfBreadcrumbs];

        for (var i = 0; i < numberOfBreadcrumbs; i++)
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

  abstract class CocoaBreadcrumbs : IBreadcrumbs
  {
    IntPtr NativeBreadcrumbs { get; }

    protected delegate void BreadcrumbInformation(IntPtr instance, string name, string timestamp, string type, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)]string[] keys, long keysSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 7)]string[] values, long valuesSize);

    internal CocoaBreadcrumbs(IntPtr nativeBreadcrumbs)
    {
      NativeBreadcrumbs = nativeBreadcrumbs;
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

    public void Leave(Breadcrumb breadcrumb)
    {
      if (breadcrumb != null)
      {
        if (breadcrumb.Metadata != null)
        {
          var index = 0;
          var metadata = new string[breadcrumb.Metadata.Count * 2];

          foreach (var data in breadcrumb.Metadata)
          {
            metadata[index] = data.Key;
            metadata[index + 1] = data.Value;
            index += 2;
          }

          AddBreadcrumb(NativeBreadcrumbs, breadcrumb.Name, breadcrumb.Type, metadata, metadata.Length);
        }
        else
        {
          AddBreadcrumb(NativeBreadcrumbs, breadcrumb.Name, breadcrumb.Type, null, 0);
        }
      }
    }

    protected abstract void AddBreadcrumb(IntPtr breadcrumbs, string name, string type, string[] metadata, int metadataCount);

    public Breadcrumb[] Retrieve()
    {
      var breadcrumbs = new List<Breadcrumb>();

      var handle = GCHandle.Alloc(breadcrumbs);

      try
      {
        RetrieveBreadcrumbs(NativeBreadcrumbs, GCHandle.ToIntPtr(handle), PopulateBreadcrumb);
      }
      finally
      {
        handle.Free();
      }

      return breadcrumbs.ToArray();
    }

    protected abstract void RetrieveBreadcrumbs(IntPtr breadcrumbs, IntPtr instance, BreadcrumbInformation visitor);

    [MonoPInvokeCallback(typeof(BreadcrumbInformation))]
    static void PopulateBreadcrumb(IntPtr instance, string name, string timestamp, string type, string[] keys, long keysSize, string[] values, long valuesSize)
    {
      var handle = GCHandle.FromIntPtr(instance);
      if (handle.Target is List<Breadcrumb> breadcrumbs)
      {
        var metadata = new Dictionary<string, string>();

        for (var i = 0; i < keys.Length; i++)
        {
          metadata.Add(keys[i], values[i]);
        }

        breadcrumbs.Add(new Breadcrumb(name, timestamp, type, metadata));
      }
    }
  }

  class MacOsBreadcrumbs : CocoaBreadcrumbs
  {
    [DllImport("bugsnag-osx", EntryPoint = "bugsnag_createBreadcrumbs")]
    static extern IntPtr CreateBreadcrumbs(IntPtr configuration);

    [DllImport("bugsnag-osx", EntryPoint = "bugsnag_addBreadcrumb")]
    static extern void NativeAddBreadcrumb(IntPtr breadcrumbs, string name, string type, string[] metadata, int metadataCount);

    [DllImport("bugsnag-osx", EntryPoint = "bugsnag_retrieveBreadcrumbs")]
    static extern void NativeRetrieveBreadcrumbs(IntPtr breadcrumbs, IntPtr instance, BreadcrumbInformation visitor);

    internal MacOsBreadcrumbs(MacOSConfiguration configuration) : base(CreateBreadcrumbs(configuration.NativeConfiguration))
    {
    }

    protected override void AddBreadcrumb(IntPtr breadcrumbs, string name, string type, string[] metadata, int metadataCount)
    {
      NativeAddBreadcrumb(breadcrumbs, name, type, metadata, metadataCount);
    }

    protected override void RetrieveBreadcrumbs(IntPtr breadcrumbs, IntPtr instance, BreadcrumbInformation visitor)
    {
      NativeRetrieveBreadcrumbs(breadcrumbs, instance, visitor);
    }
  }

  class iOSBreadcrumbs : CocoaBreadcrumbs
  {
    [DllImport("__Internal", EntryPoint = "bugsnag_createBreadcrumbs")]
    static extern IntPtr CreateBreadcrumbs(IntPtr configuration);

    [DllImport("__Internal", EntryPoint = "bugsnag_addBreadcrumb")]
    static extern void NativeAddBreadcrumb(IntPtr breadcrumbs, string name, string type, string[] metadata, int metadataCount);

    [DllImport("__Internal", EntryPoint = "bugsnag_retrieveBreadcrumbs")]
    static extern void NativeRetrieveBreadcrumbs(IntPtr breadcrumbs, IntPtr instance, BreadcrumbInformation visitor);

    internal iOSBreadcrumbs(iOSConfiguration configuration) : base(CreateBreadcrumbs(configuration.NativeConfiguration))
    {
    }

    protected override void AddBreadcrumb(IntPtr breadcrumbs, string name, string type, string[] metadata, int metadataCount)
    {
      NativeAddBreadcrumb(breadcrumbs, name, type, metadata, metadataCount);
    }

    protected override void RetrieveBreadcrumbs(IntPtr breadcrumbs, IntPtr instance, BreadcrumbInformation visitor)
    {
      NativeRetrieveBreadcrumbs(breadcrumbs, instance, visitor);
    }
  }
}
