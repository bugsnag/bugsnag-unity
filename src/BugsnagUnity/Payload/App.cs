using System;
using System.Collections.Generic;

namespace BugsnagUnity.Payload
{
    /// <summary>
    /// Represents the "app" key in the error report payload.
    /// </summary>
    public class App : Dictionary<string, object>, IFilterable
    {
        internal App(IConfiguration configuration)
        {
            Version = configuration.AppVersion;
            ReleaseStage = configuration.ReleaseStage;
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
    }
}
