#if UNITY_ANDROID && !UNITY_EDITOR

using System;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    internal class NativeAppWithState : NativeApp, IAppWithState
    {
        public NativeAppWithState(AndroidJavaObject androidJavaObject) : base(androidJavaObject){}

        public TimeSpan? Duration { get => GetNativeTimespan("getDuration"); set => SetNativeTimespan("setDuration",value); }
        public TimeSpan? DurationInForeground { get => GetNativeTimespan("getDurationInForeground"); set => SetNativeTimespan("setDurationInForeground", value); }
        public bool? InForeground { get => GetNativeBool("getInForeground"); set => SetNativeBool("setInForeground",value); }
        public bool? IsLaunching { get => GetNativeBool("isLaunching"); set => SetNativeBool("setLaunching", value); }
    }
}
#endif