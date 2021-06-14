using System;
using UnityEngine;
using System.Collections.Generic;

namespace BugsnagUnity
{
    public class MaximumLogTypeCounter
    {
        private IConfiguration Configuration { get; }

        private Dictionary<LogType, int> CurrentCounts { get; }

        private DateTime FlushAt { get; set; }

        private TimeSpan MaximumLogsTimePeriod => Configuration.MaximumLogsTimePeriod;

        private Dictionary<LogType, int> MaximumTypePerTimePeriod => Configuration.MaximumTypePerTimePeriod;

        private readonly object _lock = new object();

        public MaximumLogTypeCounter(IConfiguration configuration)
        {
            Configuration = configuration;
            CurrentCounts = new Dictionary<LogType, int>();
            FlushAt = DateTime.UtcNow.Add(Configuration.MaximumLogsTimePeriod);
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
                            FlushAt = DateTime.UtcNow.Add(MaximumLogsTimePeriod);
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
