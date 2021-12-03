using System;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    public class NativeSession : NativePayloadClassWrapper, ISession
    {
        public NativeSession(AndroidJavaObject androidJavaObject) : base(androidJavaObject){}

        public string Id { get => GetNativeString("getId"); set => SetNativeString("setId",value); }

        public IDevice Device { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IApp App { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime? StartedAt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IUser GetUser() => new NativeUser(NativePointer.Call<AndroidJavaObject>("getUser"));

        public void SetUser(string id, string email, string name)
        {
            NativePointer.Call("setUser",id,email,name);
        }
    }
}
