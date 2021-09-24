using System;
using UnityEngine;

namespace BugsnagUnity.Payload
{
    public class AppWithState : App
    {
        private const string DURATION_KEY = "duration";
        private const string DURATION_IN_FOREGROUND_KEY = "durationInForeground";
        private const string IN_FOREGROUND_KEY = "inForeground";
        private const string IS_LAUNCHING_KEY = "isLaunching";

        public TimeSpan? Duration
        {
            get
            {
                if (HasKey(DURATION_KEY))
                {
                    return TimeSpan.FromMilliseconds((double)Payload.Get(DURATION_KEY));
                }
                else
                {
                    return null;
                }
            }
            set => Add(DURATION_KEY, value?.TotalMilliseconds);
        }

        public TimeSpan? DurationInForeground
        {
            get
            {
                if (HasKey(DURATION_IN_FOREGROUND_KEY))
                {
                    return TimeSpan.FromMilliseconds((double)Payload.Get(DURATION_IN_FOREGROUND_KEY));
                }
                else
                {
                    return null;
                }
            }
            set => Add(DURATION_IN_FOREGROUND_KEY, value?.TotalMilliseconds);
        }

        public bool? InForeground
        {
            get
            {
                if (HasKey(IN_FOREGROUND_KEY))
                {
                    return (bool)Payload.Get(IN_FOREGROUND_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(IN_FOREGROUND_KEY, value);
        }

        public bool? IsLaunching
        {
            get
            {
                if (HasKey(IS_LAUNCHING_KEY))
                {
                    return (bool)Payload.Get(IS_LAUNCHING_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(IS_LAUNCHING_KEY, value);
        }

        public AppWithState(IConfiguration configuration) : base(configuration)
        {
            Duration = TimeSpan.FromSeconds(Time.realtimeSinceStartup);
        }
      
    }
}
