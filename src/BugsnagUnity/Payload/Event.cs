using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BugsnagUnity.Payload
{
    public class Event : PayloadContainer, IEvent
    {
        HandledState _handledState;

        internal HandledState OriginalSeverity { get; }

        private string[] _androidProjectPackages;

        internal Event(string context, Metadata metadata, AppWithState app, DeviceWithState device, User user, Error[] errors, HandledState handledState, List<Breadcrumb> breadcrumbs, Session session, string apiKey ,LogType? logType = null)
        {
            OriginalSeverity = handledState;
            _metadata = metadata;
            HandledState = handledState;
            LogType = logType;
            _appWithState = app;
            _deviceWithState = device;
            Context = context;
            _errors = errors.ToList();
            Errors = new List<IError>();
            foreach (var error in _errors)
            {
                Errors.Add(error);
            }
            _breadcrumbs = breadcrumbs;
            Breadcrumbs = new List<IBreadcrumb>();
            foreach (var crumb in _breadcrumbs)
            {
                Breadcrumbs.Add(crumb);
            }
            ApiKey = apiKey;
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

        internal int PayloadVersion = 4;

        public string ApiKey { get; set; }

        private List<Breadcrumb> _breadcrumbs;

        public List<IBreadcrumb> Breadcrumbs { get; }

        internal Session Session { get; }

        internal LogType? LogType { get; }

        public bool? Unhandled { get => (bool)Get("unhandled"); set => Add("unhandled",value); }

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

        private AppWithState _appWithState;

        public IAppWithState App => _appWithState;

        private DeviceWithState _deviceWithState;

        public IDeviceWithState Device => _deviceWithState;

        private List<Error> _errors;

        public List<IError> Errors { get; }

        public string GroupingHash { get; set; }
        
        public Severity Severity
        {
            set => HandledState = HandledState.ForCallbackSpecifiedSeverity(value, _handledState);
            get => _handledState.Severity;
        }



        private User _user;

        public IUser GetUser() => _user;

        public void SetUser(string id, string name, string email)
        {
            _user = new User(id, name, email );
        }

        internal bool IsAndroidJavaError()
        {
            foreach (var error in _errors)
            {
                if (error.IsAndroidJavaException)
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

        public List<IThread> Threads => null;


        internal Dictionary<string, object> GetEventPayload()
        {
            Add("app", _appWithState.Payload);
            Add("device", _deviceWithState.Payload);
            Add("metaData", _metadata.Payload);
            Add("user", _user.Payload);
            Add("context", Context);
            Add("groupingHash", GroupingHash);
            Add("payloadVersion", PayloadVersion);
            Add("exceptions", _errors);
            if (_androidProjectPackages != null)
            {
                Add("projectPackages", _androidProjectPackages);
            }
            var breadcrumbPayloads = new List<Dictionary<string, object>>();
            foreach (var crumb in _breadcrumbs)
            {
                breadcrumbPayloads.Add(crumb.Payload);
            }
            Add("breadcrumbs", breadcrumbPayloads.ToArray());
            if (Session != null)
            {
                Add("session", Session.Payload);
            }

            return Payload;
        }

    }
}
