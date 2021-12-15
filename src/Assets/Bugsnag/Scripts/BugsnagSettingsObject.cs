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
        public int MaximumBreadcrumbs = 25;
        public int MaxPersistedEvents = 32;
        public string NotifyEndpoint;
        public EditorLogLevel NotifyLogLevel = EditorLogLevel.Exception;
        public bool PersistUser;
        public string SessionEndpoint;
        public ThreadSendPolicy SendThreads = ThreadSendPolicy.UNHANDLED_ONLY;
        public string[] RedactedKeys = new string[] { "password" };
        public string ReleaseStage = "production";
        public bool ReportExceptionLogsAsHandled = true;
        public bool SendLaunchCrashesSynchronously = true;
        public double SecondsPerUniqueLog = 5;
        public int VersionCode;
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
            config.DiscardClasses = DiscardClasses;
            if (EnabledReleaseStages != null && EnabledReleaseStages.Length > 0)
            {
                config.EnabledReleaseStages = EnabledReleaseStages;
            }
            config.EnabledErrorTypes = EnabledErrorTypes;
            config.EnabledBreadcrumbTypes = GetEnabledBreadcrumbTypes();
            config.LaunchDurationMillis = LaunchDurationMillis;
            config.MaximumBreadcrumbs = MaximumBreadcrumbs;
            config.MaxPersistedEvents = MaxPersistedEvents;
            config.NotifyLogLevel = GetLogTypeFromLogLevel( NotifyLogLevel );
            config.SendThreads = SendThreads;
            if (!string.IsNullOrEmpty(NotifyEndpoint) && !string.IsNullOrEmpty(SessionEndpoint))
            {
                config.Endpoints = new EndpointConfiguration(NotifyEndpoint, SessionEndpoint);
            }
            config.RedactedKeys = RedactedKeys;
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
            config.VersionCode = VersionCode;
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
            if (EnabledBreadcrumbTypes.Manual)
            {
                enabledTypes.Add(BreadcrumbType.Manual);
            }
            return enabledTypes.ToArray();
        }

        [Serializable]
        public class EditorBreadcrumbTypes
        {
            public bool Navigation = true;
            public bool Request = true;
            public bool Process = true;
            public bool Log = true;
            public bool User = true;
            public bool State = true;
            public bool Error = true;
            public bool Manual = true;
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
