using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BugsnagUnity
{
  class Configuration : IConfiguration
  {
    const string DefaultEndpoint = "https://notify.bugsnag.com";

    const string DefaultSessionEndpoint = "https://sessions.bugsnag.com";

    internal IntPtr NativeConfiguration { get; }

    internal Configuration(string apiKey)
    {
      NativeConfiguration = NativeCode.CreateConfiguration(apiKey);
      Endpoint = new Uri(DefaultEndpoint);
      AutoNotify = true;
      SessionEndpoint = new Uri(DefaultSessionEndpoint);
      MaximumBreadcrumbs = 25;
      ReleaseStage = "production";
      NotifyLevel = LogType.Exception;
      UniqueLogsTimePeriod = TimeSpan.FromSeconds(5);
      MaximumLogsTimePeriod = TimeSpan.FromSeconds(1);
      AppVersion = Application.version;
      LogTypeSeverityMapping = new LogTypeSeverityMapping();
    }

    public string ApiKey => Marshal.PtrToStringAuto(NativeCode.GetApiKey(NativeConfiguration));
    public TimeSpan MaximumLogsTimePeriod { get; }
    public Dictionary<LogType, int> MaximumTypePerTimePeriod { get; } = new Dictionary<LogType, int>
    {
      { LogType.Assert, 5 },
      { LogType.Error, 5 },
      { LogType.Exception, 20 },
      { LogType.Log, 5 },
      { LogType.Warning, 5 },
    };
    public TimeSpan UniqueLogsTimePeriod { get; set; }
    public int MaximumBreadcrumbs { get; set; }

    public string ReleaseStage
    {
      get => Marshal.PtrToStringAuto(NativeCode.GetReleaseStage(NativeConfiguration));
      set => NativeCode.SetReleaseStage(NativeConfiguration, value);
    }

    public string[] NotifyReleaseStages
    {
      get
      {
        var releaseStages = new List<string>();

        var handle = GCHandle.Alloc(releaseStages);

        try
        {
          NativeCode.GetNotifyReleaseStages(NativeConfiguration, GCHandle.ToIntPtr(handle), PopulateReleaseStages);
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
      set => NativeCode.SetNotifyReleaseStages(NativeConfiguration, value, value.Length);
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

    public string AppVersion
    {
      get => Marshal.PtrToStringAuto(NativeCode.GetAppVersion(NativeConfiguration));
      set => NativeCode.SetAppVersion(NativeConfiguration, value);
    }

    public Uri Endpoint
    {
      get => new Uri(Marshal.PtrToStringAuto(NativeCode.GetNotifyEndpoint(NativeConfiguration)));
      set => NativeCode.SetNotifyEndpoint(NativeConfiguration, value.ToString());
    }
    public string PayloadVersion { get; } = "4.0";
    public string SessionPayloadVersion { get; } = "1";
    public Uri SessionEndpoint { get; set; }

    public string Context
    {
      get => Marshal.PtrToStringAuto(NativeCode.GetContext(NativeConfiguration));
      set => NativeCode.SetContext(NativeConfiguration, value);
    }

    public LogType NotifyLevel { get; set; }
    public bool AutoNotify { get; set; }

    public bool AutoCaptureSessions { get; set; }

    public LogTypeSeverityMapping LogTypeSeverityMapping { get; }
  }
}
