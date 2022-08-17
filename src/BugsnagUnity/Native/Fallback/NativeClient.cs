using BugsnagUnity.Payload;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity
{
    class NativeClient : INativeClient, IFeatureFlagStore
    {
        public Configuration Configuration { get; }

        public IBreadcrumbs Breadcrumbs { get; }

        private bool _launchMarkedAsCompleted = false;

        private bool _hasReceivedLowMemoryWarning = false;

        private Metadata _fallbackMetadata = new Metadata();

        private List<FeatureFlag> _featureFlags = new List<FeatureFlag>();

        public NativeClient(Configuration configuration)
        {
            Configuration = configuration;
            Breadcrumbs = new Breadcrumbs(configuration);
            Application.lowMemory += () => { _hasReceivedLowMemoryWarning = true; };
            AddFeatureFlags(configuration.FeatureFlags.ToArray());
        }

        public void PopulateApp(App app)
        {
        }

        public void PopulateAppWithState(AppWithState app)
        {
            AddIsLaunching(app);
            app.Add("lowMemory", _hasReceivedLowMemoryWarning);
        }

        private void AddIsLaunching(AppWithState app)
        {
            bool isLaunching;
            if (Configuration.LaunchDurationMillis == 0)
            {
                isLaunching = !_launchMarkedAsCompleted;
            }
            else
            {
                isLaunching = app.DurationInForeground?.TotalMilliseconds < Configuration.LaunchDurationMillis;
            }
            app.IsLaunching = isLaunching;
        }

        public void PopulateDevice(Device device)
        {
        }

        public void PopulateDeviceWithState(DeviceWithState device)
        {
        }

        public void PopulateUser(User user)
        {
        }

        public void SetSession(Session session)
        {
        }

        public void SetUser(User user)
        {
        }
        public void SetContext(string context)
        {
        }
        public void SetAutoDetectErrors(bool autoDetectErrors)
        {
        }

        public void SetAutoDetectAnrs(bool autoDetectAnrs)
        {
        }

        public void StartSession()
        {
        }

        public void PauseSession()
        {
        }

        public bool ResumeSession()
        {
            return false;
        }

        public void UpdateSession(Session session)
        {
        }

        public Session GetCurrentSession()
        {
            return null;
        }

        public void MarkLaunchCompleted()
        {
            _launchMarkedAsCompleted = true;
        }

        public LastRunInfo GetLastRunInfo()
        {
            return null;
        }

        public void ClearNativeMetadata(string section)
        {
            _fallbackMetadata.ClearMetadata(section);
        }

        public void ClearNativeMetadata(string section, string key)
        {
            _fallbackMetadata.ClearMetadata(section, key);
        }

        public void AddNativeMetadata(string section, IDictionary<string, object> data)
        {
            _fallbackMetadata.AddMetadata(section, data);
        }

        public IDictionary<string, object> GetNativeMetadata()
        {
            return _fallbackMetadata.Payload;
        }

        public void AddFeatureFlag(string name, string variant = null)
        {
            _featureFlags.Add(new FeatureFlag(name, variant));
        }

        public void AddFeatureFlags(FeatureFlag[] featureFlags)
        {
            _featureFlags.AddRange(featureFlags);
        }

        public void ClearFeatureFlag(string name)
        {
            foreach (var flag in _featureFlags.ToArray())
            {
                if (flag.Name == name)
                {
                    _featureFlags.Remove(flag);
                }
            }
        }

        public void ClearFeatureFlags()
        {
            _featureFlags.Clear();
        }

        public List<FeatureFlag> GetFeatureFlags()
        {
            return _featureFlags;
        }

        public bool ShouldAttemptDelivery()
        {
            return true;
        }
    }
}
