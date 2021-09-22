using BugsnagUnity.Payload;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity
{
    class NativeClient : INativeClient
    {
        public IConfiguration Configuration { get; }

        public IBreadcrumbs Breadcrumbs { get; }

        public IDelivery Delivery { get; }

        private Dictionary<string, object> _fallbackMetadata = new Dictionary<string, object>();

        private bool _launchMarkedAsCompleted = false;

        private bool _hasReceivedLowMemoryWarning = false;

        public NativeClient(IConfiguration configuration)
        {
            Configuration = configuration;
            Breadcrumbs = new Breadcrumbs(configuration);
            Delivery = new Delivery();
            Application.lowMemory += () => { _hasReceivedLowMemoryWarning = true; };
        }

        public void PopulateApp(App app)
        {
            AddIsLaunching(app);
            app.AddToPayload("lowMemory", _hasReceivedLowMemoryWarning);
        }

        private void AddIsLaunching(App app)
        {
            if (!app.ContainsKey("durationInForeground"))
            {
                return;
            }
            bool isLaunching;
            if (Configuration.LaunchDurationMillis == 0)
            {
                isLaunching = _launchMarkedAsCompleted;
            }
            else
            {
                isLaunching = app.DurationInForeground.Milliseconds < Configuration.LaunchDurationMillis;
            }
            app.IsLaunching = isLaunching;
        }

        public void PopulateDevice(Device device)
        {
        }

        public void PopulateMetadata(Metadata metadata)
        {
            MergeDictionaries(metadata, _fallbackMetadata);
        }
        private void MergeDictionaries(Dictionary<string, object> dest, Dictionary<string, object> another)
        {
            foreach (var entry in another)
            {
                dest.AddToPayload(entry.Key, entry.Value);
            }
        }

        public void PopulateUser(User user)
        {
        }

        public void SetMetadata(string tab, Dictionary<string, string> metadata)
        {
            _fallbackMetadata[tab] = metadata;
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
    }
}
