using System;
using System.Collections.Generic;

namespace BugsnagUnity
{
    public class UniqueLogThrottle
    {
        private readonly object _lock = new object();
        private Dictionary<UnityLogMessage, int> Counter { get; }
        private double FlushAt { get; set; } = -1;
        private Configuration Configuration { get; }

        private double UniqueLogsTimePeriod => Configuration.SecondsPerUniqueLog.TotalSeconds;

        private void EnsureFlushTimeIsSet()
        {
            if (FlushAt < 0)
            {
                FlushAt = Time.ElapsedSeconds + UniqueLogsTimePeriod;
            }
        }

        public UniqueLogThrottle(Configuration configuration)
        {
            Configuration = configuration;
            Counter = new Dictionary<UnityLogMessage, int>(new UnityLogMessageEqualityComparer());
        }

        public bool ShouldSend(UnityLogMessage unityLogMessage)
        {
            bool shouldSend;
            lock (_lock)
            {
                EnsureFlushTimeIsSet();
                shouldSend = !Counter.ContainsKey(unityLogMessage);
                if (shouldSend)
                {
                    Counter.Add(unityLogMessage, 0);
                }
                else
                {
                    if (unityLogMessage.CreatedAt > FlushAt)
                    {
                        Counter.Clear();
                        FlushAt = Time.ElapsedSeconds + UniqueLogsTimePeriod;
                        shouldSend = true;
                    }
                }
            }
            return shouldSend;
        }

        class UnityLogMessageEqualityComparer : EqualityComparer<UnityLogMessage>
        {
            public override bool Equals(UnityLogMessage x, UnityLogMessage y)
            {
                return x.Condition == y.Condition && x.StackTrace == y.StackTrace && x.Type == y.Type;
            }

            public override int GetHashCode(UnityLogMessage obj)
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + obj.Condition.GetHashCode();
                    hash = hash * 23 + obj.StackTrace.GetHashCode();
                    hash = hash * 23 + obj.Type.GetHashCode();
                    return hash;
                }
            }
        }
    }
}
