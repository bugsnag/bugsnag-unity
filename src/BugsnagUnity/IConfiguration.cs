using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity
{
  public interface IConfiguration
  {
    string ApiKey { get; }

    bool ReportUncaughtExceptionsAsHandled { get; set; }

    TimeSpan MaximumLogsTimePeriod { get; }

    Dictionary<LogType, int> MaximumTypePerTimePeriod { get; }

    TimeSpan UniqueLogsTimePeriod { get; set; }

    int MaximumBreadcrumbs { get; set; }

    string ReleaseStage { get; set; }

    string[] NotifyReleaseStages { get; set; }

    string AppVersion { get; set; }

    Uri Endpoint { get; set; }

    string PayloadVersion { get; }

    Uri SessionEndpoint { get; set; }

    string SessionPayloadVersion { get; }

    string Context { get; set; }

    LogType NotifyLevel { get; set; }

    bool AutoNotify { get; set; }

    bool AutoCaptureSessions { get; set; }

    LogTypeSeverityMapping LogTypeSeverityMapping { get; }

    bool DetectAnrs { get; set; }

    long AnrThresholdMs { get; set; }
  }
}
