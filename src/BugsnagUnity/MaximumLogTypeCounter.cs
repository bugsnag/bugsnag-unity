using System;
using UnityEngine;
using System.Collections.Generic;

namespace BugsnagUnity
{
    public class MaximumLogTypeCounter
    {
        private Configuration Configuration { get; }

        private Dictionary<LogType, int> CurrentCounts { get; }

        private float FlushAt { get; set; }

        private float MaximumLogsTimePeriod => (float)Configuration.MaximumLogsTimePeriod.TotalSeconds;

        private Dictionary<LogType, int> MaximumTypePerTimePeriod => Configuration.MaximumTypePerTimePeriod;

        private readonly object _lock = new object();

        public MaximumLogTypeCounter(Configuration configuration)
        {
            Configuration = configuration;
            CurrentCounts = new Dictionary<LogType, int>();
            FlushAt = Time.realtimeSinceStartup + MaximumLogsTimePeriod;
        }

        public bool ShouldSend(UnityLogMessage unityLogMessage)
        {
            var type = unityLogMessage.Type;

            lock (_lock)
            {
                if (MaximumTypePerTimePeriod.ContainsKey(type))
                {
                    if (CurrentCounts.ContainsKey(type))
                    {
                        CurrentCounts[type]++;
                    }
                    else
                    {
                        CurrentCounts[type] = 1;
                    }

                    if (CurrentCounts[type] > MaximumTypePerTimePeriod[type])
                    {
                        if (unityLogMessage.CreatedAt > FlushAt)
                        {
                            CurrentCounts.Clear();
                            FlushAt = Time.realtimeSinceStartup + MaximumLogsTimePeriod;
                            return true;
                        }
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
