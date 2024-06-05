using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using BugsnagUnity.Payload;
using UnityEngine;
using System.Text.RegularExpressions;

namespace BugsnagUnity
{

    public enum SwitchCacheType
    {
        None,
        R,
        I
    }

    public class Configuration : IMetadataEditor, IFeatureFlagStore
    {

        // Nintendo switch specifics ----------
        public SwitchCacheType SwitchCacheType = SwitchCacheType.R;

        public string SwitchCacheMountName = "BugsnagCache";

        public int SwitchCacheIndex = 0;

        public int SwitchCacheMaxSize = 10485760; //10MiB in Bytes
        // ----------

        public string AppType;

        public string BundleVersion;



        public int VersionCode = -1;

        public long LaunchDurationMillis = 5000;

        public ThreadSendPolicy SendThreads = ThreadSendPolicy.UnhandledOnly;

        public bool SendLaunchCrashesSynchronously = true;

        public bool GenerateAnonymousId = true;

        public bool PersistUser = true;

        public string HostName;

        private User _user = null;

        internal Metadata Metadata = new Metadata();

        internal OrderedDictionary FeatureFlags = new OrderedDictionary();


        public List<Regex> RedactedKeys = new List<Regex>{new Regex(".*password.*",RegexOptions.IgnoreCase)};
        public bool KeyIsRedacted(string key)
        {
            if (RedactedKeys == null || RedactedKeys.Count == 0)
            {
                return false;
            }
            foreach (var regex in RedactedKeys)
            {
                if (regex.IsMatch(key))
                {
                    return true;
                }
            }
            return false;
        }

        public List<Regex> DiscardClasses = new List<Regex>();

        internal bool ErrorClassIsDiscarded(string className)
        {
            if (DiscardClasses == null || DiscardClasses.Count == 0)
            {
                return false;
            }
            foreach (var regex in DiscardClasses)
            {
                if (regex.IsMatch(className))
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

        public bool ReportExceptionLogsAsHandled = true;

        public TimeSpan MaximumLogsTimePeriod = TimeSpan.FromSeconds(1);

        public Dictionary<LogType, int> MaximumTypePerTimePeriod = new Dictionary<LogType, int>
        {
            { LogType.Assert, 5 },
            { LogType.Error, 5 },
            { LogType.Exception, 20 },
            { LogType.Log, 5 },
            { LogType.Warning, 5 },
        };

        public TimeSpan SecondsPerUniqueLog = TimeSpan.FromSeconds(5);

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

        private int _maximumBreadcrumbs = 100;

        public int MaximumBreadcrumbs
        {
            get { return _maximumBreadcrumbs; }
            set
            {
                if (value < 0 || value > 500)
                {
                    if (IsRunningInEditor())
                    {
                        Debug.LogError("Invalid configuration value detected. Option maxBreadcrumbs should be an integer between 0-500. Supplied value is " + value);
                    }
                    return;
                }
                else
                {
                    _maximumBreadcrumbs = value;
                }
            }
        }

        public int MaxReportedThreads = 200;

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

        public int MaxPersistedEvents = 32;

        public int MaxPersistedSessions = 128;

        public int MaxStringValueLength = 10000;

        private bool IsRunningInEditor()
        {
            return Application.platform == RuntimePlatform.OSXEditor
                || Application.platform == RuntimePlatform.WindowsEditor
                || Application.platform == RuntimePlatform.LinuxEditor;
        }

        private List<Func<IEvent, bool>> _onErrorCallbacks = new List<Func<IEvent, bool>>();

        public void AddOnError(Func<IEvent, bool> callback)
        {
            _onErrorCallbacks.Add(callback);
        }

        internal List<Func<IEvent, bool>> GetOnErrorCallbacks()
        {
            return _onErrorCallbacks;
        }

        public void RemoveOnError(Func<IEvent, bool> callback)
        {
            _onErrorCallbacks.Remove(callback);
        }

        private List<Func<IEvent, bool>> _onSendErrorCallbacks = new List<Func<IEvent, bool>>();

        public void AddOnSendError(Func<IEvent, bool> callback)
        {
            _onSendErrorCallbacks.Add(callback);
        }

        internal List<Func<IEvent, bool>> GetOnSendErrorCallbacks()
        {
            return _onSendErrorCallbacks;
        }

        public void RemoveOnSendError(Func<IEvent, bool> callback)
        {
            _onSendErrorCallbacks.Remove(callback);
        }

        private List<Func<ISession, bool>> _onSessionCallbacks = new List<Func<ISession, bool>>();

        public void AddOnSession(Func<ISession, bool> callback)
        {
            _onSessionCallbacks.Add(callback);
        }

        public void RemoveOnSession(Func<ISession, bool> callback)
        {
            _onSessionCallbacks.Remove(callback);
        }

        internal List<Func<ISession, bool>> GetOnSessionCallbacks()
        {
            return _onSessionCallbacks;
        }

        public List<TelemetryType> Telemetry = new List<TelemetryType> { TelemetryType.InternalErrors, TelemetryType.Usage };

        public void AddMetadata(string section, string key, object value) => Metadata.AddMetadata(section, key, value);

        public void AddMetadata(string section, IDictionary<string, object> metadata) => Metadata.AddMetadata(section, metadata);

        public void ClearMetadata(string section) => Metadata.ClearMetadata(section);

        public void ClearMetadata(string section, string key) => Metadata.ClearMetadata(section, key);

        public IDictionary<string, object> GetMetadata(string section) => Metadata.GetMetadata(section);

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

        public void AddFeatureFlag(string name, string variant = null)
        {
            FeatureFlags[name] = variant;
        }

        public void AddFeatureFlags(FeatureFlag[] featureFlags)
        {
            foreach (var flag in featureFlags)
            {
                AddFeatureFlag(flag.Name,flag.Variant);
            }
        }

        public void ClearFeatureFlag(string name)
        {
            FeatureFlags.Remove(name);
        }

        public void ClearFeatureFlags()
        {
            FeatureFlags.Clear();
        }
    }

    [Serializable]
    public class EnabledErrorTypes
    {
        public bool ANRs = true;
        public bool AppHangs = true;
        public bool OOMs = true;
        public bool Crashes = true;
        public bool ThermalKills = true;
        public bool UnityLog = true;
        
        public EnabledErrorTypes()
        { }
    }
}

