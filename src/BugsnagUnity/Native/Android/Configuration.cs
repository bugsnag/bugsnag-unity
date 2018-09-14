using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity
{
  class Configuration : IConfiguration
  {
    const string DefaultEndpoint = "https://notify.bugsnag.com";

    const string DefaultSessionEndpoint = "https://sessions.bugsnag.com";

    internal AndroidJavaObject JavaObject { get; }

    internal Configuration(string apiKey)
    {
      JavaObject = new AndroidJavaObject("com.bugsnag.android.Configuration", apiKey);
        // the bugsnag-unity notifier will handle session tracking
      JavaObject.Call("setAutoCaptureSessions", false);

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

    public string ApiKey
    {
      get
      {
        return JavaObject.CallStringMethod("getApiKey");
      }
    }

    public string ReleaseStage
    {
      set
      {
        JavaObject.Call("setReleaseStage", value);
      }
      get
      {
        return JavaObject.CallStringMethod("getReleaseStage");
      }
    }

    public string[] NotifyReleaseStages
    {
      set
      {
        JavaObject.Call("setNotifyReleaseStages", new object[] { value });
      }
      get
      {
        return JavaObject.Call<string[]>("getNotifyReleaseStages");
      }
    }

    public string AppVersion
    {
      set
      {
        JavaObject.Call("setAppVersion", value);
      }
      get
      {
        return JavaObject.CallStringMethod("getAppVersion");
      }
    }

    public Uri Endpoint
    {
      set
      {
        JavaObject.Call("setEndpoint", value.ToString());
      }
      get
      {
        var endpoint = JavaObject.CallStringMethod("getEndpoint");
        return new Uri(endpoint);
      }
    }

    public Uri SessionEndpoint
    {
      set
      {
        JavaObject.Call("setSessionEndpoint", value.ToString());
      }
      get
      {
        var endpoint = JavaObject.CallStringMethod("getSessionEndpoint");
        return new Uri(endpoint);
      }
    }

    public string Context
    {
      set
      {
        JavaObject.Call("setContext", value);
      }
      get
      {
        return JavaObject.CallStringMethod("getContext");
      }
    }

    public bool AutoNotify { get; set; } // how do we hook this into the android bits, this lives on the client object

    public bool AutoCaptureSessions { get; set; }

    public LogTypeSeverityMapping LogTypeSeverityMapping { get; }
  }
}
