using System;
using System.Runtime.InteropServices;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    internal class NativeSession : ISession
    {

        private IntPtr _nativeSession;

        public NativeSession(IntPtr nativeSession)
        {
            _nativeSession = nativeSession;
            App = new NativeApp(NativeCode.bugsnag_getAppFromSession(_nativeSession));
        }

        public string Id {
            get => GetId();
            set => SetId(value);
        }

        public Device Device { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IApp App { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime? StartedAt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private string GetId()
        {
            return NativeCode.bugsnag_getIdFromSession(_nativeSession);
        }

        private void SetId(string id)
        {
            NativeCode.bugsnag_setSessionId(_nativeSession, id);
        }

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
