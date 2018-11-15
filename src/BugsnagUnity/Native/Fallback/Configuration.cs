using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity
{
  class Configuration : IConfiguration
  {
    const string DefaultEndpoint = "https://notify.bugsnag.com";

    const string DefaultSessionEndpoint = "https://sessions.bugsnag.com";

    internal Configuration(string apiKey)
    {
      ApiKey = apiKey;
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

    public string ApiKey { get; }

    public int MaximumBreadcrumbs { get; set; }

    public string ReleaseStage { get; set; }

    public string[] NotifyReleaseStages { get; set; }

    public string AppVersion { get; set; }

    public Uri Endpoint { get; set; }

    public string PayloadVersion { get; } = "4.0";

    public Uri SessionEndpoint { get; set; }

    public string SessionPayloadVersion { get; } = "1";

    public string Context { get; set; }

    public LogType NotifyLevel { get; set; }

    public bool AutoNotify { get; set; }

    public bool AutoCaptureSessions { get; set; }
    
    public LogTypeSeverityMapping LogTypeSeverityMapping { get; }
  }
}
