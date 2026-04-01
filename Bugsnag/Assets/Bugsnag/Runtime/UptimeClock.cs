using System;
using System.Diagnostics;
using UnityEngine;

namespace BugsnagUnity
{
    internal static class UptimeClock
    {
        private static readonly object _lock = new object();
        private static Stopwatch _stopwatch;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Start()
        {
            EnsureStarted();
        }

        internal static TimeSpan Elapsed => EnsureStarted().Elapsed;

        private static Stopwatch EnsureStarted()
        {
            if (_stopwatch != null)
            {
                return _stopwatch;
            }

            lock (_lock)
            {
                if (_stopwatch == null)
                {
                    _stopwatch = Stopwatch.StartNew();
                }
                return _stopwatch;
            }
        }
    }
}
