using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public class NativeThread : NativePayloadClassWrapper , IThread
    {

        private const string ID_KEY = "id";
        private const string ERROR_REPORTING_THREAD_KEY = "errorReportingThread";
        private const string NAME_KEY = "name";

        public NativeThread(IntPtr nativePointer) : base(nativePointer)
        {
            var stacktraceHandle = GCHandle.Alloc(_stacktrace);
            NativeCode.bugsnag_getStackframesFromThread(nativePointer, GCHandle.ToIntPtr(stacktraceHandle), GetStacktrace);
        }

        public string Type { get => NativeCode.bugsnag_getThreadTypeFromThread(NativePointer); }

        public string Id { get => GetNativeString(ID_KEY); set => SetNativeString(ID_KEY, value); }

        public bool? ErrorReportingThread => GetNativeBool(ERROR_REPORTING_THREAD_KEY);

        public string Name { get => GetNativeString(NAME_KEY); set => SetNativeString(NAME_KEY, value); }

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
