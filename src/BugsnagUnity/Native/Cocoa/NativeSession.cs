using System;
using System.Runtime.InteropServices;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    internal class NativeSession : NativePayloadClassWrapper, ISession
    {

        private IntPtr _nativeSession;

        private const string ID_KEY = "id";
        private const string STARTED_AT_KEY = "startedAt";

        public NativeSession(IntPtr nativeSession) : base(nativeSession)
        {
            _nativeSession = nativeSession;
            App = new NativeApp(NativeCode.bugsnag_getAppFromSession(_nativeSession));
            Device = new NativeDevice(NativeCode.bugsnag_getDeviceFromSession(_nativeSession));
        }

        public string Id {
            get => GetNativeString(ID_KEY);
            set => SetNativeString(ID_KEY, value);
        }

        public IApp App { get; set; }

        public IDevice Device { get; set; }

        public DateTime? StartedAt { get => GetNativeTimestamp(STARTED_AT_KEY); set => SetNativeTimeStamp(value, STARTED_AT_KEY); }

        public User GetUser()
        {
            var nativeUser = new NativeUser();
            NativeCode.bugsnag_populateUserFromSession(_nativeSession, ref nativeUser);
            var user = new User()
            {
                Id = Marshal.PtrToStringAuto(nativeUser.Id),
                Name = Marshal.PtrToStringAuto(nativeUser.Name),
                Email = Marshal.PtrToStringAuto(nativeUser.Email)
            };
            return user;
        }

        public void SetUser(string id, string email, string name)
        {            
            NativeCode.bugsnag_setUserFromSession(_nativeSession,id,name,email);
        }
    }
}
