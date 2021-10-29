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
            App = new NativeApp(NativeCode.bugsnag_getAppFromSession(NativePointer));
            Device = new NativeDevice(NativeCode.bugsnag_getDeviceFromSession(NativePointer));
        }

        public string Id {
            get => GetNativeString(ID_KEY);
            set => SetNativeString(ID_KEY, value);
        }

        public IApp App { get; set; }

        public IDevice Device { get; set; }

        public DateTime? StartedAt { get => GetNativeDate(STARTED_AT_KEY); set => SetNativeDate(value, STARTED_AT_KEY); }

        public User GetUser()
        {
            var nativeUser = new NativeUser();
            NativeCode.bugsnag_populateUserFromSession(NativePointer, ref nativeUser);
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
            NativeCode.bugsnag_setUserFromSession(NativePointer, id,name,email);
        }
    }
}
