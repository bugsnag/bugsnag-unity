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

        public string BundleVersion { get; set; }

        public string AppType { get; set; }

        public Configuration(string apiKey)
        {
            ApiKey = apiKey;
            AppVersion = Application.version;
            AutoTrackSessions = true;
            AutoDetectErrors = true;
            AutoDetectAnrs = true;
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

        private int _maximumBreadcrumbs = 25;

        public virtual int MaximumBreadcrumbs
        {
            get { return _maximumBreadcrumbs; }
            set
            {
                if (value < 0 || value > 100)
                {
                    if (IsRunningInEditor())
                    {
                        Debug.LogError("Invalid configuration value detected. Option maxBreadcrumbs should be an integer between 0-100. Supplied value is " + value);
                    }
                    return;
                }
                else
                {
                    _maximumBreadcrumbs = value;
                }
            }
        }

        public virtual string ReleaseStage { get; set; } = "production";

        public virtual string[] NotifyReleaseStages { get; set; }

        public virtual string AppVersion { get; set; }

        public virtual Uri Endpoint { get; set; } = new Uri(DefaultEndpoint);

        public virtual string PayloadVersion { get; } = "4.0";

        public virtual Uri SessionEndpoint { get; set; } = new Uri(DefaultSessionEndpoint);

        public virtual string SessionPayloadVersion { get; } = "1.0";

        public virtual string Context { get; set; }


        private LogType _notifyLogLevel = LogType.Exception;

        [Obsolete("NotifyLevel is deprecated, please use NotifyLogLevel instead.", false)]
        public virtual LogType NotifyLevel {
            get
            {
                return _notifyLogLevel;
            }
            set
            {
                _notifyLogLevel = value;
            }
        }

        public virtual LogType NotifyLogLevel
        {
            get
            {
                return _notifyLogLevel;
            }
            set
            {
                _notifyLogLevel = value;
            }
        }

        private bool _autoDetectErrors = true;

        [Obsolete("AutoNotify is deprecated, please use AutoDetectErrors instead.", false)]
        public virtual bool AutoNotify
        {
            get { return _autoDetectErrors; }
            set { _autoDetectErrors = value; }
        }

        public virtual bool AutoDetectErrors
        {
            get { return _autoDetectErrors; }
            set { _autoDetectErrors = value; }
        }

        public virtual bool AutoDetectAnrs { get; set; } = true;

        private bool _autoTrackSessions = true;

        [Obsolete("AutoCaptureSessions is deprecated, please use AutoTrackSessions instead.", false)]
        public virtual bool AutoCaptureSessions
        {
            get { return _autoTrackSessions; }
            set { _autoTrackSessions = value; }
        }

        public virtual bool AutoTrackSessions
        {
            get { return _autoTrackSessions; }
            set { _autoTrackSessions = value; }
        }

        public virtual LogTypeSeverityMapping LogTypeSeverityMapping { get; } = new LogTypeSeverityMapping();

        public virtual string ScriptingBackend { get; set; }

        public virtual string DotnetScriptingRuntime { get; set; }

        public virtual string DotnetApiCompatibility { get; set; }

        public ErrorTypes[] EnabledErrorTypes { get; set; }


        private ulong _appHangThresholdMillis = 0;

        public ulong AppHangThresholdMillis
        {
            get { return _appHangThresholdMillis; }
            set
            {
                if (value < 250)
                {
                    if (IsRunningInEditor())
                    {
                        Debug.LogError("Invalid configuration value detected. Option AppHangThresholdMillis should be a ulong higher than 249. Supplied value is " + value);
                    }
                    return;
                }
                else
                {
                    _appHangThresholdMillis = value;
                }
            }
        }

        public string[] DiscardClasses { get; set; }

        public virtual bool ErrorClassIsDiscarded(string className)
        {
            return DiscardClasses != null && DiscardClasses.Contains(className);
        }

        public virtual bool IsErrorTypeEnabled(ErrorTypes errorType)
        {
            return EnabledErrorTypes == null || EnabledErrorTypes.Contains(errorType);
        }

        public virtual bool IsUnityLogErrorTypeEnabled(LogType logType)
        {
            if (EnabledErrorTypes == null)
            {
                return true;
            }
            switch (logType)
            {
                case LogType.Error:
                    return EnabledErrorTypes.Contains(ErrorTypes.UnityErrorLogs);
                case LogType.Warning:
                    return EnabledErrorTypes.Contains(ErrorTypes.UnityWarningLogs);
                case LogType.Log:
                    return EnabledErrorTypes.Contains(ErrorTypes.UnityLogLogs);
                case LogType.Exception:
                    return EnabledErrorTypes.Contains(ErrorTypes.UnhandledExceptions);
                case LogType.Assert:
                    return EnabledErrorTypes.Contains(ErrorTypes.UnityAssertLogs);
                default:
                    return false;
            }
        }

        private bool IsRunningInEditor()
        {
            return Application.platform == RuntimePlatform.OSXEditor
                || Application.platform == RuntimePlatform.WindowsEditor
                || Application.platform == RuntimePlatform.LinuxEditor;
        }
    }
}

