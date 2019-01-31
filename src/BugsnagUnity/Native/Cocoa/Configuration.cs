using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BugsnagUnity
{
  class Configuration : AbstractConfiguration
  {
    internal IntPtr NativeConfiguration { get; }

    internal Configuration(string apiKey) : base()
    {
      NativeConfiguration = NativeCode.bugsnag_createConfiguration(apiKey);
      SetupDefaults(apiKey);
    }

    protected override void SetupDefaults(string apiKey)
    {
      base.SetupDefaults(apiKey);
      ReleaseStage = "production";
      Endpoint = new Uri(DefaultEndpoint);
    }

    public override string ApiKey
    {
      get => Marshal.PtrToStringAuto(NativeCode.bugsnag_getApiKey(NativeConfiguration));
      protected set {}
    }

    public override string ReleaseStage
    {
      get => Marshal.PtrToStringAuto(NativeCode.bugsnag_getReleaseStage(NativeConfiguration));
      set => NativeCode.bugsnag_setReleaseStage(NativeConfiguration, value);
    }

    public override string[] NotifyReleaseStages
    {
      get
      {
        var releaseStages = new List<string>();

        var handle = GCHandle.Alloc(releaseStages);

        try
        {
          NativeCode.bugsnag_getNotifyReleaseStages(NativeConfiguration, GCHandle.ToIntPtr(handle), PopulateReleaseStages);
        }
        finally
        {
          handle.Free();
        }

        if (releaseStages.Count == 0)
        {
          return null;
        }
        return releaseStages.ToArray();
      }
      set => NativeCode.bugsnag_setNotifyReleaseStages(NativeConfiguration, value, value.Length);
    }

    [MonoPInvokeCallback(typeof(NativeCode.NotifyReleaseStageCallback))]
    static void PopulateReleaseStages(IntPtr instance, string[] releaseStages, long count)
    {
      var handle = GCHandle.FromIntPtr(instance);
      if (handle.Target is List<string> releaseStage)
      {
        releaseStage.AddRange(releaseStages);
      }
    }

    public override string AppVersion
    {
      get => Marshal.PtrToStringAuto(NativeCode.bugsnag_getAppVersion(NativeConfiguration));
      set => NativeCode.bugsnag_setAppVersion(NativeConfiguration, value);
    }

    public override Uri Endpoint
    {
      get => new Uri(Marshal.PtrToStringAuto(NativeCode.bugsnag_getNotifyUrl(NativeConfiguration)));
      set => NativeCode.bugsnag_setNotifyUrl(NativeConfiguration, value.ToString());
    }

    public override string Context
    {
      get => Marshal.PtrToStringAuto(NativeCode.bugsnag_getContext(NativeConfiguration));
      set => NativeCode.bugsnag_setContext(NativeConfiguration, value);
    }
  }
}
