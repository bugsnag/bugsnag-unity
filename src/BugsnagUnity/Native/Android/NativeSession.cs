using System;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    public class NativeSession : NativePayloadClassWrapper, ISession
    {
        public NativeSession(AndroidJavaObject androidJavaObject) : base(androidJavaObject){}

        public string Id { get => GetNativeString("getId"); set => SetNativeString("setId",value); }

        public IDevice Device { get => new NativeDevice(NativePointer.Call<AndroidJavaObject>("getDevice")); set { } }

        public IApp App { get => new NativeApp(NativePointer.Call<AndroidJavaObject>("getApp")); set { } }

        public DateTime? StartedAt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IUser GetUser() => new NativeUser(NativePointer.Call<AndroidJavaObject>("getUser"));

        public void SetUser(string id, string email, string name)
        {
            NativePointer.Call("setUser",id,email,name);
        }
    }
}
