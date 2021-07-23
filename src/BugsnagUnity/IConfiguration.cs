using System;
using System.Collections.Generic;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    public interface IConfiguration
    {
        string ApiKey { get; }

        bool ReportUncaughtExceptionsAsHandled { get; set; }

        ErrorTypes EnabledErrorTypes { get; set; }

        bool IsUnityErrorTypeEnabled(LogType logType);

        TimeSpan MaximumLogsTimePeriod { get; }

        LogType BreadcrumbLogLevel { get; set; }

        BreadcrumbType[] EnabledBreadcrumbTypes { get; set; }

        bool IsBreadcrumbTypeEnabled(BreadcrumbType breadcrumbType);

        bool ShouldLeaveLogBreadcrumb(LogType logType);

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

        bool AutoDetectAnrs { get; set; }

        bool AutoCaptureSessions { get; set; }

        LogTypeSeverityMapping LogTypeSeverityMapping { get; }
        string ScriptingBackend { get; set; }

        string DotnetScriptingRuntime { get; set; }

        string DotnetApiCompatibility { get; set; }
    }
}
