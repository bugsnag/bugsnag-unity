using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity
{
  public abstract class AbstractConfiguration : IConfiguration
  {
    public const string DefaultEndpoint = "https://notify.bugsnag.com";

    public const string DefaultSessionEndpoint = "https://sessions.bugsnag.com";

    private long _anrThresholdMs = 5000;

    public AbstractConfiguration() {
    }

    protected virtual void SetupDefaults(string apiKey)
    {
      ApiKey = apiKey;
      AppVersion = Application.version;
    }

    public virtual bool ReportUncaughtExceptionsAsHandled { get; set; } = true;

    public virtual TimeSpan MaximumLogsTimePeriod { get; set; } = TimeSpan.FromSeconds(1);

    public virtual Dictionary<LogType, int> MaximumTypePerTimePeriod { get; set; } = new Dictionary<LogType, int>
    {
        { LogType.Assert, 5 },
        { LogType.Error, 5 },
        { LogType.Exception, 20 },
        { LogType.Log, 5 },
        { LogType.Warning, 5 },
    };

    public virtual TimeSpan UniqueLogsTimePeriod { get; set; } = TimeSpan.FromSeconds(5);

    public virtual string ApiKey { get; protected set; }

    public virtual int MaximumBreadcrumbs { get; set; } = 25;

    public virtual string ReleaseStage { get; set; } = "production";

    public virtual string[] NotifyReleaseStages { get; set; }

    public virtual string AppVersion { get; set; }

    public virtual Uri Endpoint { get; set; } = new Uri(DefaultEndpoint);

    public virtual string PayloadVersion { get; } = "4.0";

    public virtual Uri SessionEndpoint { get; set; } = new Uri(DefaultSessionEndpoint);

    public virtual string SessionPayloadVersion { get; } = "1.0";

    public virtual string Context { get; set; }

    public virtual LogType NotifyLevel { get; set; } = LogType.Exception;

    public virtual bool AutoNotify { get; set; } = true;

    public virtual bool AutoCaptureSessions { get; set; }

    public virtual LogTypeSeverityMapping LogTypeSeverityMapping { get; } = new LogTypeSeverityMapping();

    public virtual bool DetectAnrs { get; set; }

    public virtual long AnrThresholdMs {
      get => _anrThresholdMs;
      set => _anrThresholdMs = value < 1000 ? 1000 : value;
    }
  }
}

