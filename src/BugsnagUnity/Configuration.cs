using System;
using System.Collections.Generic;
using System.Linq;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    public class Configuration : IConfiguration
    {
        public const string DefaultEndpoint = "https://notify.bugsnag.com";

        public const string DefaultSessionEndpoint = "https://sessions.bugsnag.com";

        public Configuration(string apiKey)
        {
            ApiKey = apiKey;
            AppVersion = Application.version;
            AutoCaptureSessions = true;
            AutoNotify = true;
            ReleaseStage = "production";
            Endpoint = new Uri(DefaultEndpoint);
            SessionEndpoint = new Uri(DefaultSessionEndpoint);
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

        public virtual LogType BreadcrumbLogLevel { get; set; } = LogType.Log;

        public virtual bool ShouldLeaveLogBreadcrumb(LogType logType)
        {
            return IsBreadcrumbTypeEnabled(BreadcrumbType.Log)
                && logType.IsGreaterThanOrEqualTo(BreadcrumbLogLevel);
        }

        public BreadcrumbType[] EnabledBreadcrumbTypes { get; set; }

        public virtual bool IsBreadcrumbTypeEnabled(BreadcrumbType breadcrumbType)
        {
            return EnabledBreadcrumbTypes == null ||
               EnabledBreadcrumbTypes.Contains(breadcrumbType);
        }

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

        public virtual bool AutoDetectAnrs { get; set; } = false;

        public virtual bool AutoCaptureSessions { get; set; }

        public virtual LogTypeSeverityMapping LogTypeSeverityMapping { get; } = new LogTypeSeverityMapping();

        public virtual string ScriptingBackend { get; set; }

        public virtual string DotnetScriptingRuntime { get; set; }

        public virtual string DotnetApiCompatibility { get; set; }
    }
}

