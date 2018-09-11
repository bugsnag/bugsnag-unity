using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
  class CocoaBreadcrumbs : IBreadcrumbs
  {
    IntPtr NativeBreadcrumbs { get; }

    ICocoaWrapper Wrapper { get; }

    protected delegate void BreadcrumbInformation(IntPtr instance, string name, string timestamp, string type, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)]string[] keys, long keysSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 7)]string[] values, long valuesSize);

    CocoaBreadcrumbs(IntPtr nativeConfiguration, ICocoaWrapper wrapper)
    {
      Wrapper = wrapper;
      NativeBreadcrumbs = wrapper.CreateBreadcrumbs(nativeConfiguration);
    }

    internal CocoaBreadcrumbs(MacOSConfiguration configuration) : this(configuration.NativeConfiguration, new MacOSWrapper())
    {
    }

    internal CocoaBreadcrumbs(iOSConfiguration configuration) : this(configuration.NativeConfiguration, new iOSWrapper())
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

          Wrapper.NativeAddBreadcrumb(NativeBreadcrumbs, breadcrumb.Name, breadcrumb.Type, metadata, metadata.Length);
        }
        else
        {
          Wrapper.NativeAddBreadcrumb(NativeBreadcrumbs, breadcrumb.Name, breadcrumb.Type, null, 0);
        }
      }
    }

    public Breadcrumb[] Retrieve()
    {
      var breadcrumbs = new List<Breadcrumb>();

      var handle = GCHandle.Alloc(breadcrumbs);

      try
      {
        Wrapper.NativeRetrieveBreadcrumbs(NativeBreadcrumbs, GCHandle.ToIntPtr(handle), PopulateBreadcrumb);
      }
      finally
      {
        handle.Free();
      }

      return breadcrumbs.ToArray();
    }

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

    interface ICocoaWrapper
    {
      IntPtr CreateBreadcrumbs(IntPtr configuration);

      void NativeAddBreadcrumb(IntPtr breadcrumbs, string name, string type, string[] metadata, int metadataCount);

      void NativeRetrieveBreadcrumbs(IntPtr breadcrumbs, IntPtr instance, BreadcrumbInformation visitor);
    }

    class MacOSWrapper : ICocoaWrapper
    {
      IntPtr ICocoaWrapper.CreateBreadcrumbs(IntPtr configuration) => CreateBreadcrumbs(configuration);

      void ICocoaWrapper.NativeAddBreadcrumb(IntPtr breadcrumbs, string name, string type, string[] metadata, int metadataCount) => NativeAddBreadcrumb(breadcrumbs, name, type, metadata, metadataCount);

      void ICocoaWrapper.NativeRetrieveBreadcrumbs(IntPtr breadcrumbs, IntPtr instance, BreadcrumbInformation visitor) => NativeRetrieveBreadcrumbs(breadcrumbs, instance, visitor);

      [DllImport("bugsnag-osx", EntryPoint = "bugsnag_createBreadcrumbs")]
      static extern IntPtr CreateBreadcrumbs(IntPtr configuration);

      [DllImport("bugsnag-osx", EntryPoint = "bugsnag_addBreadcrumb")]
      static extern void NativeAddBreadcrumb(IntPtr breadcrumbs, string name, string type, string[] metadata, int metadataCount);

      [DllImport("bugsnag-osx", EntryPoint = "bugsnag_retrieveBreadcrumbs")]
      static extern void NativeRetrieveBreadcrumbs(IntPtr breadcrumbs, IntPtr instance, BreadcrumbInformation visitor);
    }

    class iOSWrapper : ICocoaWrapper
    {
      IntPtr ICocoaWrapper.CreateBreadcrumbs(IntPtr configuration) => CreateBreadcrumbs(configuration);

      void ICocoaWrapper.NativeAddBreadcrumb(IntPtr breadcrumbs, string name, string type, string[] metadata, int metadataCount) => NativeAddBreadcrumb(breadcrumbs, name, type, metadata, metadataCount);

      void ICocoaWrapper.NativeRetrieveBreadcrumbs(IntPtr breadcrumbs, IntPtr instance, BreadcrumbInformation visitor) => NativeRetrieveBreadcrumbs(breadcrumbs, instance, visitor);

      [DllImport("__Internal", EntryPoint = "bugsnag_createBreadcrumbs")]
      static extern IntPtr CreateBreadcrumbs(IntPtr configuration);

      [DllImport("__Internal", EntryPoint = "bugsnag_addBreadcrumb")]
      static extern void NativeAddBreadcrumb(IntPtr breadcrumbs, string name, string type, string[] metadata, int metadataCount);

      [DllImport("__Internal", EntryPoint = "bugsnag_retrieveBreadcrumbs")]
      static extern void NativeRetrieveBreadcrumbs(IntPtr breadcrumbs, IntPtr instance, BreadcrumbInformation visitor);
    }
  }
}
