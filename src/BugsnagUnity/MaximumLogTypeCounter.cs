using System;
using UnityEngine;
using System.Collections.Generic;

namespace BugsnagUnity
{
    public class MaximumLogTypeCounter
    {
        private Configuration Configuration { get; }

        private Dictionary<LogType, int> CurrentCounts { get; }

        private double FlushAt { get; set; } = -1; 

        private double MaximumLogsTimePeriod => Configuration.MaximumLogsTimePeriod.TotalSeconds;

        private Dictionary<LogType, int> MaximumTypePerTimePeriod => Configuration.MaximumTypePerTimePeriod;

        private readonly object _lock = new object();

        private void EnsureFlushTimeIsSet()
        {
            if (FlushAt < 0) 
            {
                FlushAt = Time.ElapsedSeconds + MaximumLogsTimePeriod;
            }
        }

        public MaximumLogTypeCounter(Configuration configuration)
        {
            Configuration = configuration;
            CurrentCounts = new Dictionary<LogType, int>();
        }

        public bool ShouldSend(UnityLogMessage unityLogMessage)
        {
            var type = unityLogMessage.Type;

            lock (_lock)
            {
                EnsureFlushTimeIsSet();

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
                            FlushAt = Time.ElapsedSeconds + MaximumLogsTimePeriod;
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
