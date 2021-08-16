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

        string[] DiscardClasses { get; set; }

        string[] ProjectPackages { get; set; }

        bool ErrorClassIsDiscarded(string className);

        ErrorTypes[] EnabledErrorTypes { get; set; }

        bool IsUnityLogErrorTypeEnabled(LogType logType);

        bool IsErrorTypeEnabled(ErrorTypes errorType);

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

        LogType NotifyLogLevel { get; set; }
        
        bool AutoDetectErrors { get; set; }

        bool AutoDetectAnrs { get; set; }

        bool AutoTrackSessions { get; set; }

        LogTypeSeverityMapping LogTypeSeverityMapping { get; }
        string ScriptingBackend { get; set; }

        string DotnetScriptingRuntime { get; set; }

        string DotnetApiCompatibility { get; set; }

        ulong AppHangThresholdMillis { get; set; }

        string BundleVersion { get; set; }

        string AppType { get; set; }
    }
}
