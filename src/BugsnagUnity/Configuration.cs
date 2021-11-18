using System;
using System.Collections.Generic;
using System.Linq;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    public class Configuration : IMetadataEditor
    {

        public string AppType;

        public string BundleVersion;

        public string[] RedactedKeys = new string[] { "password" };

        public int VersionCode = -1;

        public long LaunchDurationMillis = 5000;

        public ThreadSendPolicy SendThreads = ThreadSendPolicy.UNHANDLED_ONLY;

        public bool SendLaunchCrashesSynchronously = true;

        public bool GenerateAnonymousId = true;

        public bool PersistUser;

        public string HostName;

        private User _user = null;

        internal Metadata Metadata = new Metadata();

        public bool KeyIsRedacted(string key)
        {
            if (RedactedKeys == null || RedactedKeys.Length == 0)
            {
                return false;
            }
            foreach (var redactedKey in RedactedKeys)
            {
                if (key.ToLower() == redactedKey.ToLower())
                {
                    return true;
                }
            }
            return false;
        }

        public Configuration(string apiKey)
        {
            ApiKey = apiKey;
        }

        public string PersistenceDirectory;

        public bool ReportUncaughtExceptionsAsHandled = true;

        public TimeSpan MaximumLogsTimePeriod = TimeSpan.FromSeconds(1);

        public Dictionary<LogType, int> MaximumTypePerTimePeriod = new Dictionary<LogType, int>
        {
            { LogType.Assert, 5 },
            { LogType.Error, 5 },
            { LogType.Exception, 20 },
            { LogType.Log, 5 },
            { LogType.Warning, 5 },
        };

        public TimeSpan UniqueLogsTimePeriod = TimeSpan.FromSeconds(5);

        public LogType BreadcrumbLogLevel = LogType.Log;

        public bool ShouldLeaveLogBreadcrumb(LogType logType)
        {
            return IsBreadcrumbTypeEnabled(BreadcrumbType.Log)
                && logType.IsGreaterThanOrEqualTo(BreadcrumbLogLevel);
        }

        public BreadcrumbType[] EnabledBreadcrumbTypes { get; set; }

        public bool IsBreadcrumbTypeEnabled(BreadcrumbType breadcrumbType)
        {
            return EnabledBreadcrumbTypes == null ||
               EnabledBreadcrumbTypes.Contains(breadcrumbType);
        }

        public string ApiKey { get; set; }

        private int _maximumBreadcrumbs = 25;

        public int MaximumBreadcrumbs
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

        public string ReleaseStage = "production";

        public string[] EnabledReleaseStages;

        public string[] ProjectPackages;

        public string AppVersion = Application.version;

        public EndpointConfiguration Endpoints = new EndpointConfiguration();

        internal string PayloadVersion { get; } = "4.0";

        internal string SessionPayloadVersion { get; } = "1.0";

        public string Context;

        public LogType NotifyLogLevel = LogType.Exception;

        public bool AutoDetectErrors = true;

        public bool AutoTrackSessions = true;

        public LogTypeSeverityMapping LogTypeSeverityMapping { get; } = new LogTypeSeverityMapping();

        public string ScriptingBackend;

        public string DotnetScriptingRuntime;

        public string DotnetApiCompatibility;

        public EnabledErrorTypes EnabledErrorTypes = new EnabledErrorTypes();

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

        public string[] DiscardClasses;

        public int MaxPersistedEvents = 32;

        internal bool ErrorClassIsDiscarded(string className)
        {
            return DiscardClasses != null && DiscardClasses.Contains(className);
        }

        internal bool IsUnityLogErrorTypeEnabled(LogType logType)
        {
            if (!AutoDetectErrors)
            {
                return false;
            }
            switch (logType)
            {
                case LogType.Error:
                    return EnabledErrorTypes.UnityErrorLogs;
                case LogType.Warning:
                    return EnabledErrorTypes.UnityWarningLogs;
                case LogType.Log:
                    return EnabledErrorTypes.UnityLogLogs;
                case LogType.Exception:
                    return EnabledErrorTypes.UnhandledExceptions;
                case LogType.Assert:
                    return EnabledErrorTypes.UnityAssertLogs;
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


        private List<OnErrorCallback> _onErrorCallbacks = new List<OnErrorCallback>();

        public void AddOnError(OnErrorCallback callback)
        {
            _onErrorCallbacks.Add(callback);
        }

        internal List<OnErrorCallback> GetOnErrorCallbacks()
        {
            return _onErrorCallbacks;
        }

        public void RemoveOnError(OnErrorCallback callback)
        {
            _onErrorCallbacks.Remove(callback);
        }

        private List<OnSendErrorCallback> _onSendErrorCallbacks = new List<OnSendErrorCallback>();

        public void AddOnSendError(OnSendErrorCallback callback)
        {
            _onSendErrorCallbacks.Add(callback);
        }

        internal List<OnSendErrorCallback> GetOnSendErrorCallbacks()
        {
            return _onSendErrorCallbacks;
        }

        public void RemoveOnSendError(OnSendErrorCallback callback)
        {
            _onSendErrorCallbacks.Remove(callback);
        }

        private List<OnSessionCallback> _onSessionCallbacks = new List<OnSessionCallback>();

        public void AddOnSession(OnSessionCallback callback)
        {
            _onSessionCallbacks.Add(callback);
        }

        public void RemoveOnSession(OnSessionCallback callback)
        {
            _onSessionCallbacks.Remove(callback);
        }

        internal List<OnSessionCallback> GetOnSessionCallbacks()
        {
            return _onSessionCallbacks;
        }

        public void AddMetadata(string section, string key, object value) => Metadata.AddMetadata(section, key, value);

        public void AddMetadata(string section, Dictionary<string, object> metadata) => Metadata.AddMetadata(section, metadata);

        public void ClearMetadata(string section) => Metadata.ClearMetadata(section);

        public void ClearMetadata(string section, string key) => Metadata.ClearMetadata(section, key);

        public Dictionary<string, object> GetMetadata(string section) => Metadata.GetMetadata(section);

        public object GetMetadata(string section, string key) => Metadata.GetMetadata(section, key);

        public User GetUser() => _user;

        public void SetUser(string id, string email, string name)
        {
            _user = new User(id, email, name);
        }

        internal Configuration Clone()
        {
            var clone = (Configuration)MemberwiseClone();
            if (_user != null)
            {
                clone._user = _user.Clone();
            }
            if (Endpoints.IsValid)
            {
                clone.Endpoints = Endpoints.Clone();
            }
            return clone;
        }

    }

    [Serializable]
    public class EnabledErrorTypes
    {
        public bool ANRs = true;
        public bool AppHangs = true;
        public bool OOMs = true;
        public bool NativeCrashes = true;
        public bool UnhandledExceptions = true;
        public bool UnityLogLogs = false;
        public bool UnityWarningLogs = false;
        public bool UnityAssertLogs = false;
        public bool UnityErrorLogs = true;

        public EnabledErrorTypes()
        { }
    }
}

