using System;
using System.Runtime.InteropServices;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    internal class NativeSession : NativePayloadClassWrapper, ISession
    {

        private const string ID_KEY = "id";
        private const string STARTED_AT_KEY = "startedAt";

        public NativeSession(IntPtr nativeSession) : base(nativeSession)
        {
            App = new NativeApp(NativeCode.bugsnag_getAppFromSession(nativeSession));
            Device = new NativeDevice(NativeCode.bugsnag_getDeviceFromSession(nativeSession));
            _nativeUser = new NativeUser(NativeCode.bugsnag_getUserFromSession(nativeSession));
        }

        public string Id {
            get => GetNativeString(ID_KEY);
            set => SetNativeString(ID_KEY, value);
        }

        public IApp App { get; set; }

        public IDevice Device { get; set; }

        public DateTime? StartedAt { get => GetNativeDate(STARTED_AT_KEY); set => SetNativeDate(value, STARTED_AT_KEY); }

        private NativeUser _nativeUser;

        public IUser GetUser() => _nativeUser;
        
        public void SetUser(string id, string email, string name)
        {
            NativeCode.bugsnag_setUserFromSession(NativePointer, id, name, email);
        }
    }
}