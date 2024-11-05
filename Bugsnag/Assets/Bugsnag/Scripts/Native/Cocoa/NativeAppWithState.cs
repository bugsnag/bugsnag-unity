#if (UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR

using System;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public class NativeAppWithState : NativeApp, IAppWithState
    {

        private const string DURATION_KEY = "duration";
        private const string DURATION_IN_FOREGROUND_KEY = "durationInForeground";
        private const string IN_FOREGROUND_KEY = "inForeground";
        private const string IS_LAUNCHING_KEY = "isLaunching";


        public NativeAppWithState(IntPtr nativeAppWithState) : base(nativeAppWithState)
        {
        }

        public TimeSpan? Duration
        {
            get
            {
                var timestamp = NativeWrapper.GetNativeDouble(DURATION_KEY);
                if (timestamp == null)
                {
                    return null;
                }
                else
                {
                    return TimeSpan.FromMilliseconds((double)timestamp);
                }
            }
            set
            {
                if (value == null)
                {
                    NativeWrapper.SetNativeDouble(DURATION_KEY, null);
                }
                else
                {
                    NativeWrapper.SetNativeDouble(DURATION_KEY,(double)value?.TotalMilliseconds);
                }
            }
        }

        public TimeSpan? DurationInForeground
        {
            get
            {
                var timestamp = NativeWrapper.GetNativeDouble(DURATION_IN_FOREGROUND_KEY);
                if (timestamp == null)
                {
                    return null;
                }
                else
                {
                    return TimeSpan.FromMilliseconds((double)timestamp);
                }
            }
            set
            {
                if (value == null)
                {
                    NativeWrapper.SetNativeDouble(DURATION_IN_FOREGROUND_KEY, null);
                }
                else
                {
                    NativeWrapper.SetNativeDouble(DURATION_IN_FOREGROUND_KEY, (double)value?.Milliseconds);
                }
            }
        }

        public bool? InForeground {
            get => NativeWrapper.GetNativeBool(IN_FOREGROUND_KEY);
            set => NativeWrapper.SetNativeBool(IN_FOREGROUND_KEY,value);
        }

        public bool? IsLaunching {
            get => NativeWrapper.GetNativeBool(IS_LAUNCHING_KEY);
            set => NativeWrapper.SetNativeBool(IS_LAUNCHING_KEY, value);
        }
    }
}
#endif