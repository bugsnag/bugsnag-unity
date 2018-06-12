using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bugsnag.Unity
{
  public class Configuration
  {
    public const string DefaultEndpoint = "https://notify.bugsnag.com";

    public const string DefaultSessionEndpoint = "https://sessions.bugsnag.com";

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

    public string ApiKey { get; }

    private Uri _endpoint;

    public Uri Endpoint
    {
      get
      {
        return _endpoint;
      }
      set
      {
        Native.Client.SetNotifyUrl(value.ToString());
        _endpoint = value;
      }
    }

    private bool _autoNotify;

    public bool AutoNotify
    {
      get
      {
        return _autoNotify;
      }
      set
      {
        Native.Client.SetAutoNotify(value);
        _autoNotify = value;
      }
    }

    private string _releaseStage;

    public string ReleaseStage
    {
      get
      {
        return _releaseStage;
      }
      set
      {
        Native.Client.SetReleaseStage(value);
        _releaseStage = value;
      }
    }

    private string[] _notifyReleaseStages;

    public string[] NotifyReleaseStages
    {
      get
      {
        return _notifyReleaseStages;
      }
      set
      {
        Native.Client.SetNotifyReleaseStages(value);
        _notifyReleaseStages = value;
      }
    }

    private string _appVersion;

    public string AppVersion
    {
      get
      {
        return _appVersion;
      }
      set
      {
        Native.Client.SetAppVersion(value);
        _appVersion = value;
      }
    }

    public Uri SessionEndpoint { get; set; }

    private int _maximumBreadcrumbs;

    public int MaximumBreadcrumbs
    {
      get
      {
        return _maximumBreadcrumbs;
      }
      set
      {
        Native.Client.SetBreadcrumbCapacity(value);
        _maximumBreadcrumbs = value;
      }
    }

    private string _context;

    public string Context
    {
      get
      {
        return _context;
      }
      set
      {
        Native.Client.SetContext(value);
        _context = value;
      }
    }

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
