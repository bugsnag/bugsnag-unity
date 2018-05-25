using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bugsnag.Unity
{
  public class Configuration
  {
    public const string DefaultEndpoint = "https://notify.bugsnag.com";

    public const string DefaultSessionEndpoint = "https://sessions.bugsnag.com";

    public Configuration() : this(string.Empty)
    {

    }

    public Configuration(string apiKey)
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
    }

    public string PayloadVersion { get; } = "4";

    public string SessionPayloadVersion { get; } = "1.0";

    public string ApiKey { get; set; }

    public Uri Endpoint { get; set; }

    public bool AutoNotify { get; set; }

    public string ReleaseStage { get; set; }

    public string[] NotifyReleaseStages { get; set; }

    public string AppVersion { get; set; }

    public Uri SessionEndpoint { get; set; }

    public int MaximumBreadcrumbs { get; set; }

    public string Context { get; set; }

    public LogType NotifyLevel { get; set; }

    public TimeSpan UniqueLogsTimePeriod { get; set; }

    public TimeSpan MaximumLogsTimePeriod { get; set; }

    public Dictionary<LogType, Severity> SeverityMapping { get; } = new Dictionary<LogType, Severity>
    {
        { LogType.Assert, Severity.Warning },
        { LogType.Error, Severity.Warning },
        { LogType.Exception, Severity.Error },
        { LogType.Log, Severity.Info },
        { LogType.Warning, Severity.Warning },
    };

    public Dictionary<LogType, int> MaximumTypePerTimePeriod { get; } = new Dictionary<LogType, int>
    {
        { LogType.Assert, 5 },
        { LogType.Error, 5 },
        { LogType.Exception, 20 },
        { LogType.Log, 5 },
        { LogType.Warning, 5 },
    };
  }
}
