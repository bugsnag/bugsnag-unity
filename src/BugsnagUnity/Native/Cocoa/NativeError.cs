using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public class NativeError : NativePayloadClassWrapper, IError
    {
        private const string ERROR_CLASS_KEY = "errorClass";
        private const string ERROR_MESSAGE_KEY = "errorMessage";

        public NativeError(IntPtr nativeError) : base (nativeError)
        {
            var stacktraceHandle = GCHandle.Alloc(_stacktrace);
            NativeCode.bugsnag_getStackframesFromError(nativeError, GCHandle.ToIntPtr(stacktraceHandle), GetStacktrace);
        }

        public string ErrorClass { get => GetNativeString(ERROR_CLASS_KEY); set => SetNativeString(ERROR_CLASS_KEY,value); }
        public string ErrorMessage { get => GetNativeString(ERROR_MESSAGE_KEY); set => SetNativeString(ERROR_MESSAGE_KEY, value); }

        public string Type { get => NativeCode.bugsnag_getErrorTypeFromError(NativePointer); }

        private List<IStackframe> _stacktrace = new List<IStackframe>();

        public List<IStackframe> Stacktrace => _stacktrace;

        [MonoPInvokeCallback(typeof(NativeCode.ErrorStackframes))]
        private static void GetStacktrace(IntPtr instance, IntPtr[] stackframePointers, int count)
        {
            var handle = GCHandle.FromIntPtr(instance);
            if (handle.Target is List<IStackframe> stacktrace)
            {
                foreach (var pointer in stackframePointers)
                {
                    stacktrace.Add(new NativeStackFrame(pointer));
                }
            }
        }

    }
}
