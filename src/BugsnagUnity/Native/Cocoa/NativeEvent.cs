using System;
using System.Collections.Generic;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public class NativeEvent : NativePayloadClassWrapper, IEvent
    {

        private const string CONTEXT_KEY = "context";

        public NativeEvent(IntPtr nativeEvent) : base(nativeEvent)
        {
            App = new NativeAppWithState(NativeCode.bugsnag_getAppFromEvent(nativeEvent));
            Device = new NativeDeviceWithState(NativeCode.bugsnag_getDeviceFromEvent(nativeEvent));
        }

        public string Context { get => GetNativeString(CONTEXT_KEY); set => SetNativeString(CONTEXT_KEY,value); }

        public IAppWithState App { get; set; }

        public IDeviceWithState Device { get; set; }

        public void AddMetadata(string section, string key, object value)
        {
            throw new NotImplementedException();
        }

        public void AddMetadata(string section, Dictionary<string, object> metadata)
        {
            throw new NotImplementedException();
        }

        public void ClearMetadata(string section)
        {
            throw new NotImplementedException();
        }

        public void ClearMetadata(string section, string key)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetMetadata(string section)
        {
            throw new NotImplementedException();
        }

        public object GetMetadata(string section, string key)
        {
            throw new NotImplementedException();
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
