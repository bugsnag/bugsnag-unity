﻿using System;
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
            get => (TimeSpan?)Get(DURATION_KEY);
            set => Add(DURATION_KEY, value?.TotalMilliseconds);
        }

        public TimeSpan? DurationInForeground
        {
            get => (TimeSpan?)Get(DURATION_IN_FOREGROUND_KEY);
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

        public AppWithState(IConfiguration configuration) : base(configuration)
        {
            Duration = TimeSpan.FromSeconds(Time.realtimeSinceStartup);
        }
      
    }
}
