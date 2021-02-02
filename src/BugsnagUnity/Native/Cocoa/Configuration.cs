using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BugsnagUnity
{
  class Configuration : AbstractConfiguration
  {
    // Cached value of native-layer auto notify configuration setting to reduce the
    // number of native calls required when reporting a Unity error
    private bool _autoNotify;

    internal IntPtr NativeConfiguration { get; }

    internal Configuration(string apiKey, bool autoNotify) : base()
    {
      NativeConfiguration = NativeCode.bugsnag_createConfiguration(apiKey);
      _autoNotify = autoNotify;
      SetupDefaults(apiKey, autoNotify);
      NativeCode.bugsnag_setAutoNotify(NativeConfiguration, autoNotify);
    }

    protected override void SetupDefaults(string apiKey, bool autoNotify)
    {
      base.SetupDefaults(apiKey, autoNotify);
      ReleaseStage = "production";
      Endpoint = new Uri(DefaultEndpoint);
    }

    public override string ApiKey
    {
      get => Marshal.PtrToStringAuto(NativeCode.bugsnag_getApiKey(NativeConfiguration));
      protected set {}
    }

    public override bool AutoNotify
    {
      get => _autoNotify;
      set {
        _autoNotify = value;
        NativeCode.bugsnag_setAutoNotify(NativeConfiguration, value);
      }
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
