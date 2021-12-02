using System;
using UnityEngine;

namespace BugsnagUnity
{
    public class NativeUser : NativePayloadClassWrapper, IUser
    {
        public NativeUser(AndroidJavaObject androidJavaObject) : base (androidJavaObject){}

        public string Id => GetNativeString("getId"); 
        public string Name => GetNativeString("getName"); 
        public string Email => GetNativeString("getEmail");

    }
}
