#if (UNITY_ANDROID && !UNITY_EDITOR) || BSG_ANDROID_DEV
using System.Collections.Generic;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    internal class NativeThread : NativePayloadClassWrapper, IThread
    {
        public NativeThread(AndroidJavaObject androidJavaObject) : base(androidJavaObject){}

        public string Id { get => GetNativeString("getId"); set => SetNativeString("setId",value); }

        public bool? ErrorReportingThread => GetNativeBool("getErrorReportingThread");

        public string Name { get => GetNativeString("getName"); set => SetNativeString("setName",value); }

        public string State 
        { 
            get => NativePointer.Call<AndroidJavaObject>("getState").Call<string>("getDescriptor"); 
            set 
            {
                // Convert string descriptor to Thread.State enum and call setState
                var threadStateClass = new AndroidJavaClass("com.bugsnag.android.Thread$State");
                var stateEnum = threadStateClass.CallStatic<AndroidJavaObject>("byDescriptor", value);
                NativePointer.Call("setState", stateEnum);
            }
        }

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