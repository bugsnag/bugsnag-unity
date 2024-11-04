#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;

namespace BugsnagUnity
{
    internal class NativeUser : NativePayloadClassWrapper, IUser
    {
        public NativeUser(AndroidJavaObject androidJavaObject) : base (androidJavaObject){}

        public string Id => GetNativeString("getId"); 
        public string Name => GetNativeString("getName"); 
        public string Email => GetNativeString("getEmail");

    }
}
#endif