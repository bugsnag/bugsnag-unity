using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bugsnag.Unity.Tests
{
  public class TestConfiguration : IConfiguration
  {
    public TimeSpan MaximumLogsTimePeriod { get; set; }

    public Dictionary<LogType, int> MaximumTypePerTimePeriod { get; set; }

    public TimeSpan UniqueLogsTimePeriod { get; set; }

    public string ApiKey { get; set; }

    public int MaximumBreadcrumbs { get; set; }

    public string ReleaseStage { get; set; }

    public string AppVersion { get; set; }

    public Uri Endpoint { get; set; }

    public string PayloadVersion { get; set; }

    public Uri SessionEndpoint { get; set; }

    public string SessionPayloadVersion { get; set; }

    public string Context { get; set; }

    public LogType NotifyLevel { get; set; }

    public bool AutoNotify { get; set; }
  }
}
