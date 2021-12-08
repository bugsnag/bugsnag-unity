using System;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    public class NativeAppWithState : NativeApp, IAppWithState
    {
        public NativeAppWithState(AndroidJavaObject androidJavaObject) : base(androidJavaObject){}

        public TimeSpan? Duration { get => GetNativeTimespan("getDuration"); set => SetNativeTimespan("setDuration",value); }
        public TimeSpan? DurationInForeground { get => GetNativeTimespan("getDurationInForeground"); set => SetNativeTimespan("setDurationInForeground", value); }
        public bool? InForeground { get => GetNativeBool("getInForeground"); set => SetNativeBool("setInForeground",value); }
        public bool? IsLaunching { get => GetNativeBool("getIsLaunching"); set => SetNativeBool("setIsLaunching", value); }
    }
}
