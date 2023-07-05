using System;
using System.Diagnostics;

namespace BugsnagUnity
{
    // A simple timer used internally to avoid thread complications when using the unity Time API
    internal class Time
    {

        private static object _swLock = new object();

        private static Stopwatch _sw;

        internal static Stopwatch Timer
        {
            get
            {
                if (_sw == null)
                {
                    lock (_swLock)
                    {
                        if (_sw == null)
                        {
                            _sw = new Stopwatch();
                            _sw.Start();
                        }
                    }
                }
                return _sw;
            }
        }

        internal static double ElapsedSeconds => Timer.Elapsed.TotalSeconds;

    }
}

