#if (UNITY_ANDROID && !UNITY_EDITOR) || BGS_ANDROID_DEV
using System;
using System.Collections.Generic;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    internal class NativeError : NativePayloadClassWrapper, IError
    {
        public NativeError(AndroidJavaObject androidJavaObject) : base(androidJavaObject){}

        public string ErrorClass { get => GetNativeString("getErrorClass"); set => SetNativeString("setErrorClass",value); }
        public string ErrorMessage { get => GetNativeString("getErrorMessage"); set => SetNativeString("setErrorMessage", value); }

        public List<IStackframe> Stacktrace => GetStacktrace();

        public string Type => NativePointer.Call<AndroidJavaObject>("getType").Call<string>("toString");

        private List<IStackframe> GetStacktrace()
        {
            var nativeList = NativePointer.Call<AndroidJavaObject>("getStacktrace");
            if (nativeList == null)
            {
                return null;
            }
            var theStacktrace = new List<IStackframe>();
            var iterator = nativeList.Call<AndroidJavaObject>("iterator");
            while (iterator.Call<bool>("hasNext"))
            {
                var next = iterator.Call<AndroidJavaObject>("next");
                theStacktrace.Add(new NativeStackFrame(next));
            }
            return theStacktrace;
        }
        
    }
}
#endif