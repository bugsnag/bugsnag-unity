﻿using BugsnagUnity.Payload;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity
{
    class NativeClient : INativeClient
    {
        public Configuration Configuration { get; }

        public IBreadcrumbs Breadcrumbs { get; }

        public IDelivery Delivery { get; }

        private Metadata _fallbackMetadata = new Metadata();

        private bool _launchMarkedAsCompleted = false;

        private bool _hasReceivedLowMemoryWarning = false;

        public NativeClient(Configuration configuration)
        {
            Configuration = configuration;
            Breadcrumbs = new Breadcrumbs(configuration);
            Delivery = new Delivery();
            Application.lowMemory += () => { _hasReceivedLowMemoryWarning = true; };
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
                isLaunching = app.DurationInForeground?.Milliseconds < Configuration.LaunchDurationMillis;
            }
            app.IsLaunching = isLaunching;
        }

        public void PopulateDevice(Device device)
        {
        }

        public void PopulateDeviceWithState(DeviceWithState device)
        {
        }

        public void PopulateMetadata(Metadata metadata)
        {
            metadata.MergeMetadata(_fallbackMetadata.Payload);
        }
       
        public void PopulateUser(User user)
        {
        }

        public void SetMetadata(string section, Dictionary<string, object> metadata)
        {
            _fallbackMetadata.AddMetadata(section,metadata);
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
