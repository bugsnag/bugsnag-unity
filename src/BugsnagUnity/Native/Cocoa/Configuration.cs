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

    internal Configuration(string apiKey) : base(apiKey)
    {
      NativeConfiguration = NativeCode.bugsnag_createConfiguration(apiKey);
    }

    public new string ApiKey => Marshal.PtrToStringAuto(NativeCode.bugsnag_getApiKey(NativeConfiguration));

    public new string ReleaseStage
    {
      get => Marshal.PtrToStringAuto(NativeCode.bugsnag_getReleaseStage(NativeConfiguration));
      set => NativeCode.bugsnag_setReleaseStage(NativeConfiguration, value);
    }

    public new string[] NotifyReleaseStages
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

    public new string AppVersion
    {
      get => Marshal.PtrToStringAuto(NativeCode.bugsnag_getAppVersion(NativeConfiguration));
      set => NativeCode.bugsnag_setAppVersion(NativeConfiguration, value);
    }

    public new Uri Endpoint
    {
      get => new Uri(Marshal.PtrToStringAuto(NativeCode.bugsnag_getNotifyUrl(NativeConfiguration)));
      set => NativeCode.bugsnag_setNotifyUrl(NativeConfiguration, value.ToString());
    }

    public new string Context
    {
      get => Marshal.PtrToStringAuto(NativeCode.bugsnag_getContext(NativeConfiguration));
      set => NativeCode.bugsnag_setContext(NativeConfiguration, value);
    }
  }
}
