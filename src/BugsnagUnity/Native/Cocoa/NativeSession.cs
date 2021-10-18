using System;
namespace BugsnagUnity
{
    internal class NativeSession : INativeSession
    {

        private IntPtr _nativeSession;

        public NativeSession(IntPtr nativeSession)
        {
            _nativeSession = nativeSession;
        }

        public string GetId()
        {
            return NativeCode.bugsnag_getIdFromSession(_nativeSession);
        }
    }
}
