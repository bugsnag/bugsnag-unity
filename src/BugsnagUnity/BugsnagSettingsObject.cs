using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BugsnagUnity;
using BugsnagUnity.Payload;
using System;

namespace BugsnagUnity.Editor
{
    public class BugsnagSettingsObject : ScriptableObject
    {
        public string ApiKey;
        public bool AutoDetectErrors = true;
        public bool AutoTrackSessions = true;
        public LogType NotifyLogLevel = LogType.Exception;
        public int MaximumBreadcrumbs = 25;
        public double SecondsPerUniqueLog = 5;
        public EnabledErrorTypes EnabledErrorTypes = new EnabledErrorTypes();

        public EditorBreadcrumbTypes EnabledBreadcrumbTypes = new EditorBreadcrumbTypes();

        //public BreadcrumbType EnabledBreadcrumbTypes ;
        public Configuration GetConfig()
        {
            var config = new Configuration(ApiKey);
            config.AutoDetectErrors = AutoDetectErrors;
            config.AutoTrackSessions = AutoTrackSessions;
            config.UniqueLogsTimePeriod = TimeSpan.FromSeconds(SecondsPerUniqueLog);
            config.NotifyLogLevel = NotifyLogLevel;
            config.ReleaseStage = Debug.isDebugBuild ? "development" : "production";
            config.MaximumBreadcrumbs = MaximumBreadcrumbs;
            config.EnabledBreadcrumbTypes = GetEnabledBreadcrumbTypes();
            config.EnabledErrorTypes = EnabledErrorTypes;
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

    }
}
