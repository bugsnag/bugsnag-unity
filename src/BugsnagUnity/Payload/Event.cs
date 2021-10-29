using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BugsnagUnity.Payload
{
    public class Event : PayloadContainer, IEvent
    {
        HandledState _handledState;

        internal HandledState OriginalSeverity { get; }

        private Dictionary<string,PayloadContainer> _payloadContainers = new Dictionary<string, PayloadContainer>();

        private string[] _androidProjectPackages;

        internal Event(string context, Metadata metadata, AppWithState app, DeviceWithState device, User user, Exception[] exceptions, HandledState handledState, List<Breadcrumb> breadcrumbs, Session session, LogType? logType = null)
        {
            OriginalSeverity = handledState;
            _metadata = metadata;
            HandledState = handledState;
            LogType = logType;
            App = app;
            _payloadContainers.Add("app", app);
            Device = device;
            _payloadContainers.Add("device", device);
            Context = context;
            Exceptions = exceptions.ToList();
            Breadcrumbs = breadcrumbs;
            _user = user;
            if (session != null)
            {
                if (handledState.Handled)
                {
                    session.Events.IncrementHandledCount();
                }
                else
                {
                    session.Events.IncrementUnhandledCount();
                }
                Session = session;
            }
        }
        internal void AddAndroidProjectPackagesToEvent(string[] packages)
        {
            _androidProjectPackages = packages;
        }

        private Metadata _metadata { get; }

        public void AddMetadata(string section, string key, object value) => _metadata.AddMetadata(section,key,value);

        public void AddMetadata(string section, Dictionary<string, object> metadata) => _metadata.AddMetadata(section, metadata);

        public Dictionary<string, object> GetMetadata(string section) => _metadata.GetMetadata(section);

        public object GetMetadata(string section, string key) => _metadata.GetMetadata(section,key);

        public void ClearMetadata(string section) => _metadata.ClearMetadata(section);

        public void ClearMetadata(string section, string key) => _metadata.ClearMetadata(section, key);

        public List<Breadcrumb> Breadcrumbs { get; }

        internal Session Session { get; }

        internal LogType? LogType { get; }

        internal bool IsHandled
        {
            get
            {
                if (Get("unhandled") is bool unhandled)
                {
                    return !unhandled;
                }

                return false;
            }
        }

        public string Context { get; set; }

        public IAppWithState App { get; }

        public IDeviceWithState Device { get; }

        public List<Exception> Exceptions { get; }

        public string GroupingHash;
        
        internal Severity Severity
        {
            set => HandledState = HandledState.ForCallbackSpecifiedSeverity(value, _handledState);
            get => _handledState.Severity;
        }

        private User _user;

        public User GetUser() => _user;

        public void SetUser(string id, string name, string email)
        {
            _user = new User(id, name, email );
        }

        internal bool IsAndroidJavaError()
        {
            foreach (var exception in Exceptions)
            {
                if (exception.IsAndroidJavaException)
                {
                    return true;
                }
            }
            return false;
        }

        HandledState HandledState
        {
            set
            {
                _handledState = value;
                foreach (var item in value)
                {
                    Payload[item.Key] = item.Value;
                }
            }
        }

        internal Dictionary<string, object> GetEventPayload()
        {

            Add("app", _payloadContainers["app"].Payload);
            Add("device", _payloadContainers["device"].Payload);
            Add("metaData", _metadata.Payload);
            Add("user", _user.Payload);
            Add("context", Context);
            Add("groupingHash", GroupingHash);
            Add("payloadVersion", 4);
            Add("exceptions", Exceptions);
            if (_androidProjectPackages != null)
            {
                Add("projectPackages", _androidProjectPackages);
            }
            var breadcrumbPayloads = new List<Dictionary<string, object>>();
            foreach (var crumb in Breadcrumbs)
            {
                breadcrumbPayloads.Add(crumb.Payload);
            }
            Add("breadcrumbs", breadcrumbPayloads);
            if (Session != null)
            {
                Add("session", Session.Payload);
            }

            return Payload;
        }

    }
}
