using System;
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
        }

        public long? Id { get => GetNativeLong(ID_KEY); set => SetNativeLong(ID_KEY, value); }

        public bool? ErrorReportingThread => GetNativeBool(ERROR_REPORTING_THREAD_KEY);

        public string Name { get => GetNativeString(NAME_KEY); set => SetNativeString(NAME_KEY, value); }
    }
}
