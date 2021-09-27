using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity.Payload
{
    /// <summary>
    /// Represents the "app" key in the error report payload.
    /// </summary>
    public class App : Dictionary<string, object>, IFilterable
    {

        private IConfiguration _configuration;

        internal App(IConfiguration configuration)
        {
            _configuration = configuration;
            Version = configuration.AppVersion;
            ReleaseStage = configuration.ReleaseStage;
            this.AddToPayload("id", Application.identifier);
            this.AddToPayload("type", GetAppType());
            AddDuration();
        }

        public string Version
        {
            get => this.Get("version") as string;
            set => this.AddToPayload("version", value);
        }

        public string ReleaseStage
        {
            get => this.Get("releaseStage") as string;
            set => this.AddToPayload("releaseStage", value);
        }

        public bool InForeground
        {
            get => (bool)this.Get("inForeground");
            set => this.AddToPayload("inForeground", value);
        }

        public TimeSpan DurationInForeground
        {
            get => TimeSpan.FromMilliseconds((double)this.Get("durationInForeground"));
            set => this.AddToPayload("durationInForeground", value.TotalMilliseconds);
        }

        private string GetAppType()
        {
            if (!string.IsNullOrEmpty(_configuration.AppType))
            {
                return _configuration.AppType;
            }
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return "MacOS";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return "Windows";
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxEditor:
                    return "Linux";
                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";
                default:
                    return string.Empty;
            }
        }

        private void AddDuration()
        {
            var timeSinceStartup = TimeSpan.FromSeconds(Time.realtimeSinceStartup);
            this.AddToPayload("duration", timeSinceStartup.TotalMilliseconds);
        }

    }
}
