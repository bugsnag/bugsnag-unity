#if (UNITY_ANDROID && !UNITY_EDITOR) || BGS_ANDROID_DEV

using System;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    internal class NativeSession : NativePayloadClassWrapper, ISession
    {
        public NativeSession(AndroidJavaObject androidJavaObject) : base(androidJavaObject){}

        public string Id { get => GetNativeString("getId"); set => SetNativeString("setId",value); }

        public IDevice Device { get => new NativeDevice(NativePointer.Call<AndroidJavaObject>("getDevice")); set { } }

        public IApp App { get => new NativeApp(NativePointer.Call<AndroidJavaObject>("getApp")); set { } }

        public DateTimeOffset? StartedAt { get => GetNativeDateTime("getStartedAt"); set => SetNativeDateTime("setStartedAt",value); }

        public IUser GetUser() => new NativeUser(NativePointer.Call<AndroidJavaObject>("getUser"));

        public void SetUser(string id, string email, string name)
        {
            NativePointer.Call("setUser",id,email,name);
        }
    }
}
#endif