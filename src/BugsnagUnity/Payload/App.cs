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

            Duration = TimeSpan.FromSeconds(Time.realtimeSinceStartup);

            Id = Application.identifier;

            Version = configuration.AppVersion;

            ReleaseStage = configuration.ReleaseStage;

            Type = GetAppType();
        }

        

        public TimeSpan Duration
        {
            get => TimeSpan.FromMilliseconds((double)this.Get("duration"));
            set => this.AddToPayload("duration", value.TotalMilliseconds);
        }

        public TimeSpan DurationInForeground
        {
            get => TimeSpan.FromMilliseconds((double)this.Get("durationInForeground"));
            set => this.AddToPayload("durationInForeground", value.TotalMilliseconds);
        }

        public string Id
        {
            get => this.Get("id") as string;
            set => this.AddToPayload("id",value);
        }

        public bool InForeground
        {
            get => (bool)this.Get("inForeground");
            set => this.AddToPayload("inForeground", value);
        }

        public bool IsLaunching
        {
            get => (bool)this.Get("isLaunching");
            set => this.AddToPayload("isLaunching", value);
        }

        public string ReleaseStage
        {
            get => this.Get("releaseStage") as string;
            set => this.AddToPayload("releaseStage", value);
        }

        public string Type
        {
            get => this.Get("type") as string;
            set => this.AddToPayload("type", value);
        }

        public string Version
        {
            get => this.Get("version") as string;
            set => this.AddToPayload("version", value);
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

    }
}
