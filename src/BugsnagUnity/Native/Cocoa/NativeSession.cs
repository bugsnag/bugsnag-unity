using System;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    internal class NativeSession : ISession
    {

        private IntPtr _nativeSession;

        public NativeSession(IntPtr nativeSession)
        {
            _nativeSession = nativeSession;
        }

        public string Id {
            get => GetId();
            set => SetId(value);
        }

        public Device Device { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public App App { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
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
            throw new NotImplementedException();
        }

        public void SetUser(string id, string email, string name)
        {
            throw new NotImplementedException();
        }
    }
}
