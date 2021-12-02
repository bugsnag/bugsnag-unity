using System;
using UnityEngine;

namespace BugsnagUnity
{
    public class NativePayloadClassWrapper
    {

        public AndroidJavaObject NativePointer;

        public NativePayloadClassWrapper(AndroidJavaObject nativePointer)
        {
            NativePointer = nativePointer;
        }

        public string GetNativeString(string key)
        {
            return NativePointer.Call<string>(key);
        }

        public void SetNativeString(string key, string value)
        {
            NativePointer.Call(key, value);
        }
    }
}
