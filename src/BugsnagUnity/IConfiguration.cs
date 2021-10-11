﻿using System;
using System.Collections.Generic;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    public interface IConfiguration
    {
        string ApiKey { get; }

        long LaunchDurationMillis { get; set; }

        bool ReportUncaughtExceptionsAsHandled { get; set; }

        bool SendLaunchCrashesSynchronously { get; set; }

        bool GenerateAnonymousId { get; set; }

        string HostName { get; set; }

        string[] DiscardClasses { get; set; }

        string[] ProjectPackages { get; set; }

        string[] RedactedKeys { get; set; }

        int VersionCode { get; set; }
      
        string PersistenceDirectory { get; set; }

        ThreadSendPolicy SendThreads { get; set; }

        bool PersistUser { get; set; }
       
        int MaxPersistedEvents { get; set; }

        bool KeyIsRedacted(string key);

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

        string[] EnabledReleaseStages { get; set; }

        string AppVersion { get; set; }

        EndpointConfiguration Endpoints { get; set; }

        string PayloadVersion { get; }

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

        void AddOnError(OnErrorCallback callback);

        void RemoveOnError(OnErrorCallback callback);

        List<OnErrorCallback> GetOnErrorCallbacks();

        void AddOnSession(OnSessionCallback callback);

        void RemoveOnSession(OnSessionCallback callback);

        List<OnSessionCallback> GetOnSessionCallbacks();

        void AddMetadata(string section, string key, object value);

        void AddMetadata(string section, Dictionary<string, object> metadata);

        void ClearMetadata(string section);

        void ClearMetadata(string section, string key);

        object GetMetadata(string section);

        object GetMetadata(string section, string key);

        Metadata Metadata { get; set; }

    }
}
