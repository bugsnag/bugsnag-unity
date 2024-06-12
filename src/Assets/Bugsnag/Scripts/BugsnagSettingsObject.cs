using System.Collections.Generic;
using UnityEngine;
using BugsnagUnity.Payload;
using System;

namespace BugsnagUnity
{
    [Serializable]
    public class BugsnagSettingsObject : ScriptableObject
    {

        public bool StartAutomaticallyAtLaunch = true;
        public bool AutoDetectErrors = true;
        public bool AutoTrackSessions = true;
        public string ApiKey;
        public string AppType;
        public ulong AppHangThresholdMillis;
        public string AppVersion;
        public string BundleVersion;
        public EditorLogLevel BreadcrumbLogLevel = EditorLogLevel.Log;
        public string Context;
        public string[] DiscardClasses;
        public string[] EnabledReleaseStages;
        public EnabledErrorTypes EnabledErrorTypes = new EnabledErrorTypes();
        public EditorBreadcrumbTypes EnabledBreadcrumbTypes = new EditorBreadcrumbTypes();
        public long LaunchDurationMillis = 5000;
        public int MaximumBreadcrumbs = 100;
        public int MaxPersistedEvents = 32;
        public int MaxPersistedSessions = 128;
        public int MaxReportedThreads = 200;
        public int MaxStringValueLength = 10000;
        public string NotifyEndpoint = "https://notify.bugsnag.com";
        public EditorLogLevel NotifyLogLevel = EditorLogLevel.Exception;
        public bool PersistUser = true;
        public string SessionEndpoint = "https://sessions.bugsnag.com";
        public ThreadSendPolicy SendThreads = ThreadSendPolicy.UnhandledOnly;
        public string[] RedactedKeys = new string[] { ".*password.*" };
        public string ReleaseStage;
        public bool ReportExceptionLogsAsHandled = true;
        public bool SendLaunchCrashesSynchronously = true;
        public double SecondsPerUniqueLog = 5;
        public List<TelemetryType> Telemetry = new List<TelemetryType> { TelemetryType.InternalErrors, TelemetryType.Usage };
        public int VersionCode = -1;

        public bool GenerateAnonymousId = true;
        public SwitchCacheType SwitchCacheType = SwitchCacheType.R;
        public string SwitchCacheMountName = "BugsnagCache";
        public int SwitchCacheIndex = 0;
        public int SwitchCacheMaxSize = 10485760;

        public static Configuration LoadConfiguration()
        {
            var settings = Resources.Load<BugsnagSettingsObject>("Bugsnag/BugsnagSettingsObject");
            if (settings != null)
            {
                var config = settings.GetConfig();
                return config;
            }
            else
            {
                throw new Exception("No BugsnagSettingsObject found.");
            }
        }

        public Configuration GetConfig()
        {
            var config = new Configuration(ApiKey);
            config.AutoDetectErrors = AutoDetectErrors;
            config.AutoTrackSessions = AutoTrackSessions;
            config.AppType = AppType;
            if (AppHangThresholdMillis > 0)
            {
                config.AppHangThresholdMillis = AppHangThresholdMillis;
            }
            if (!string.IsNullOrEmpty(AppVersion))
            {
                config.AppVersion = AppVersion;
            }
            config.BundleVersion = BundleVersion;

            config.BreadcrumbLogLevel = GetLogTypeFromLogLevel( BreadcrumbLogLevel );
            config.Context = Context;
            foreach(string discardedClass in DiscardClasses){
                try
                {
                    config.DiscardClasses.Add(new System.Text.RegularExpressions.Regex(discardedClass));
                }
                catch (ArgumentException e)
                {
                    Debug.LogError("Invalid Regex pattern for discard class: " + e.Message);
                }
            }
            if (EnabledReleaseStages != null && EnabledReleaseStages.Length > 0)
            {
                config.EnabledReleaseStages = EnabledReleaseStages;
            }
            config.EnabledErrorTypes = EnabledErrorTypes;
            config.EnabledBreadcrumbTypes = GetEnabledBreadcrumbTypes();
            config.LaunchDurationMillis = LaunchDurationMillis;
            config.MaximumBreadcrumbs = MaximumBreadcrumbs;
            config.MaxPersistedEvents = MaxPersistedEvents;
            config.MaxPersistedSessions = MaxPersistedSessions;
            config.MaxReportedThreads = MaxReportedThreads;
            config.MaxStringValueLength = MaxStringValueLength;
            config.NotifyLogLevel = GetLogTypeFromLogLevel( NotifyLogLevel );
            config.SendThreads = SendThreads;
            if (!string.IsNullOrEmpty(NotifyEndpoint) && !string.IsNullOrEmpty(SessionEndpoint))
            {
                config.Endpoints = new EndpointConfiguration(NotifyEndpoint, SessionEndpoint);
            }
            foreach(string key in RedactedKeys)
            {
                try
                {
                    config.RedactedKeys.Add(new System.Text.RegularExpressions.Regex(key));
                }
                catch (ArgumentException e)
                {
                    Debug.LogError("Invalid Regex pattern for redacted key: " + e.Message);
                }
            }
            if (string.IsNullOrEmpty(ReleaseStage))
            {
                config.ReleaseStage = Debug.isDebugBuild ? "development" : "production";
            }
            else
            {
                config.ReleaseStage = ReleaseStage;
            }
            config.PersistUser = PersistUser;
            config.ReportExceptionLogsAsHandled = ReportExceptionLogsAsHandled;
            config.SendLaunchCrashesSynchronously = SendLaunchCrashesSynchronously;
            config.SecondsPerUniqueLog = TimeSpan.FromSeconds(SecondsPerUniqueLog);
            config.Telemetry = Telemetry;
            config.VersionCode = VersionCode;
            config.GenerateAnonymousId = GenerateAnonymousId;
            config.SwitchCacheType = SwitchCacheType;
            config.SwitchCacheIndex = SwitchCacheIndex;
            config.SwitchCacheMaxSize = SwitchCacheMaxSize;
            config.SwitchCacheMountName = SwitchCacheMountName;

            return config;
        }

        private BreadcrumbType[] GetEnabledBreadcrumbTypes()
        {
            var enabledTypes = new List<BreadcrumbType>();
            if (EnabledBreadcrumbTypes.Navigation)
            {
                enabledTypes.Add(BreadcrumbType.Navigation);
            }
            if (EnabledBreadcrumbTypes.Request)
            {
                enabledTypes.Add(BreadcrumbType.Request);
            }
            if (EnabledBreadcrumbTypes.Process)
            {
                enabledTypes.Add(BreadcrumbType.Process);
            }
            if (EnabledBreadcrumbTypes.Log)
            {
                enabledTypes.Add(BreadcrumbType.Log);
            }
            if (EnabledBreadcrumbTypes.User)
            {
                enabledTypes.Add(BreadcrumbType.User);
            }
            if (EnabledBreadcrumbTypes.State)
            {
                enabledTypes.Add(BreadcrumbType.State);
            }
            if (EnabledBreadcrumbTypes.Error)
            {
                enabledTypes.Add(BreadcrumbType.Error);
            }
            enabledTypes.Add(BreadcrumbType.Manual);
            return enabledTypes.ToArray();
        }

        [Serializable]
        public class EditorBreadcrumbTypes
        {
            public bool Error = true;
            public bool Log = true;
            public bool Navigation = true;
            public bool Process = true;
            public bool Request = true;
            public bool State = true;
            public bool User = true;
        }

        [Serializable]
        public enum EditorLogLevel
        {
           Exception,
           Error,
           Assert,
           Warning,
           Log
        }


        private LogType GetLogTypeFromLogLevel(EditorLogLevel editorLogLevel)
        {
            switch (editorLogLevel)
            {
                case EditorLogLevel.Exception:
                    return LogType.Exception;
                case EditorLogLevel.Error:
                    return LogType.Error;
                case EditorLogLevel.Assert:
                    return LogType.Assert;
                case EditorLogLevel.Warning:
                    return LogType.Warning;
                default:
                    return LogType.Log;
            }
        }

    }
}
