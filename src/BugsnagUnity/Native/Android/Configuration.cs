using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity
{
  class Configuration : IConfiguration
  {
    const string DefaultEndpoint = "https://notify.bugsnag.com";

    const string DefaultSessionEndpoint = "https://sessions.bugsnag.com";

    internal NativeInterface NativeInterface { get; }

    internal Configuration(string apiKey)
    {
      ApiKey = apiKey;
      var JavaObject = new AndroidJavaObject("com.bugsnag.android.Configuration", apiKey);
      // the bugsnag-unity notifier will handle session tracking
      JavaObject.Call("setAutoCaptureSessions", false);
      JavaObject.Call("setEndpoint", DefaultEndpoint);
      JavaObject.Call("setSessionEndpoint", DefaultSessionEndpoint);
      JavaObject.Call("setReleaseStage", "production");
      JavaObject.Call("setAppVersion", Application.version);
      NativeInterface = new NativeInterface(JavaObject);

      AutoNotify = true;
      MaximumBreadcrumbs = 25;
      NotifyLevel = LogType.Exception;
      UniqueLogsTimePeriod = TimeSpan.FromSeconds(5);
      MaximumLogsTimePeriod = TimeSpan.FromSeconds(1);
      LogTypeSeverityMapping = new LogTypeSeverityMapping();
    }

    public TimeSpan MaximumLogsTimePeriod { get; set; }

    public Dictionary<LogType, int> MaximumTypePerTimePeriod { get; } = new Dictionary<LogType, int>
    {
        { LogType.Assert, 5 },
        { LogType.Error, 5 },
        { LogType.Exception, 20 },
        { LogType.Log, 5 },
        { LogType.Warning, 5 },
    };

    public TimeSpan UniqueLogsTimePeriod { get; set; }

    public int MaximumBreadcrumbs { get; set; } // this is stored on the client object in java so we need to figure out how this will work

    public LogType NotifyLevel { get; set; }

    public string PayloadVersion { get; } = "4.0";

    public string SessionPayloadVersion { get; } = "1";

    public string ApiKey { get; }

    public string ReleaseStage
    {
      set
      {
        NativeInterface.SetReleaseStage(value);
      }
      get
      {
        return NativeInterface.GetReleaseStage();
      }
    }

    public string[] NotifyReleaseStages
    {
      set
      {
        NativeInterface.SetNotifyReleaseStages(value);
      }
      get
      {
        return NativeInterface.GetNotifyReleaseStages();
      }
    }

    public string AppVersion
    {
      set
      {
        NativeInterface.SetAppVersion(value);
      }
      get
      {
        return NativeInterface.GetAppVersion();
      }
    }

    public Uri Endpoint
    {
      set
      {
        NativeInterface.SetEndpoint(value.ToString());
      }
      get
      {
        return new Uri(NativeInterface.GetEndpoint());
      }
    }

    public Uri SessionEndpoint
    {
      set
      {
        NativeInterface.SetSessionEndpoint(value.ToString());
      }
      get
      {
        return new Uri(NativeInterface.GetSessionEndpoint());
      }
    }

    public string Context
    {
      set
      {
        NativeInterface.SetContext(value);
      }
      get
      {
        return NativeInterface.GetContext();
      }
    }

    public bool AutoNotify { get; set; } // how do we hook this into the android bits, this lives on the client object

    public bool AutoCaptureSessions { get; set; }

    public LogTypeSeverityMapping LogTypeSeverityMapping { get; }
  }
}
