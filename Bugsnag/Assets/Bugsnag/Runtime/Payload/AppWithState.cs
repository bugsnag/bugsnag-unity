using System;
using System.Collections.Generic;
using UnityEngine;
#nullable enable

namespace BugsnagUnity.Payload
{
    public class AppWithState : App, IAppWithState
    {
        private const string DURATION_KEY = "duration";
        private const string DURATION_IN_FOREGROUND_KEY = "durationInForeground";
        private const string IN_FOREGROUND_KEY = "inForeground";
        private const string IS_LAUNCHING_KEY = "isLaunching";

        public TimeSpan? Duration
        {
            get
            {
                var millis = (double?)Get(DURATION_KEY);
                if (millis != null)
                {
                    return TimeSpan.FromMilliseconds((double)millis);
                }
                return null;
            }
            set => Add(DURATION_KEY, value?.TotalMilliseconds);
        }

        public TimeSpan? DurationInForeground
        {
            get
            {
                var millis = (double?)Get(DURATION_IN_FOREGROUND_KEY);
                if (millis != null)
                {
                    return TimeSpan.FromMilliseconds((double)millis);
                }
                return null;
            }
            set => Add(DURATION_IN_FOREGROUND_KEY, value?.TotalMilliseconds);
        }

        public bool? InForeground
        {
            get => (bool?)Get(IN_FOREGROUND_KEY);
            set => Add(IN_FOREGROUND_KEY, value);
        }

        public bool? IsLaunching
        {
            get => (bool?)Get(IS_LAUNCHING_KEY);
            set => Add(IS_LAUNCHING_KEY, value);
        }

        internal AppWithState(Dictionary<string, object> cachedData) : base(cachedData) { }

        internal AppWithState(Configuration configuration) : base(configuration)
        {
            Duration = TimeSpan.FromSeconds(UnityEngine.Time.realtimeSinceStartup);
        }

    }
}
