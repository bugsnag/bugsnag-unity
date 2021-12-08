using System;
using System.Collections.Generic;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    internal class NativeThread : NativePayloadClassWrapper, IThread
    {
        public NativeThread(AndroidJavaObject androidJavaObject) : base(androidJavaObject){}

        public string Id { get => GetNativeLong("getId").ToString(); set => SetNativeLong("setId",long.Parse(value)); }

        public bool? ErrorReportingThread => GetNativeBool("getErrorReportingThread");

        public string Name { get => GetNativeString("getName"); set => SetNativeString("setName",value); }

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
