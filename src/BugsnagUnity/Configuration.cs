using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity
{
  public abstract class AbstractConfiguration : IConfiguration
  {
    public const string DefaultEndpoint = "https://notify.bugsnag.com";

    public const string DefaultSessionEndpoint = "https://sessions.bugsnag.com";

    public AbstractConfiguration(string apiKey)
    {
      ApiKey = apiKey;
      AppVersion = Application.version;
    }

    public TimeSpan MaximumLogsTimePeriod { get; set; } = TimeSpan.FromSeconds(1);

    public Dictionary<LogType, int> MaximumTypePerTimePeriod { get; set; } = new Dictionary<LogType, int>
    {
        { LogType.Assert, 5 },
        { LogType.Error, 5 },
        { LogType.Exception, 20 },
        { LogType.Log, 5 },
        { LogType.Warning, 5 },
    };

    public TimeSpan UniqueLogsTimePeriod { get; set; } = TimeSpan.FromSeconds(5);

    public string ApiKey { get; }

    public int MaximumBreadcrumbs { get; set; } = 25;

    public string ReleaseStage { get; set; } = "production";

    public string[] NotifyReleaseStages { get; set; }

    public string AppVersion { get; set; }

    public Uri Endpoint { get; set; } = new Uri(DefaultEndpoint);

    public string PayloadVersion { get; } = "4.0";

    public Uri SessionEndpoint { get; set; } = new Uri(DefaultSessionEndpoint);

    public string SessionPayloadVersion { get; } = "1";

    public string Context { get; set; }

    public LogType NotifyLevel { get; set; } = LogType.Exception;

    public bool AutoNotify { get; set; } = true;

    public bool AutoCaptureSessions { get; set; }

    public LogTypeSeverityMapping LogTypeSeverityMapping { get; } = new LogTypeSeverityMapping();
  }
}

