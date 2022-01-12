using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace BugsnagUnity.Payload
{
    public class Event : PayloadContainer, IEvent
    {

        internal Event(string context, Metadata metadata, AppWithState app, DeviceWithState device, User user, Error[] errors, HandledState handledState, List<Breadcrumb> breadcrumbs, Session session, string apiKey ,LogType? logType = null)
        {
            ApiKey = apiKey;
            OriginalSeverity = handledState;
            _metadata = metadata;
            HandledState = handledState;
            LogType = logType;
            _appWithState = app;
            _deviceWithState = device;
            Context = context;
            _user = user;

            _errors = errors.ToList();
            Errors = new List<IError>();
            foreach (var error in _errors)
            {
                Errors.Add(error);
            }

            _breadcrumbs = breadcrumbs;
            var breadcrumbsList = new List<IBreadcrumb>();
            foreach (var crumb in _breadcrumbs)
            {
                breadcrumbsList.Add(crumb);
            }
            Breadcrumbs = new ReadOnlyCollection<IBreadcrumb>(breadcrumbsList);

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

        HandledState _handledState;

        internal HandledState OriginalSeverity { get; }

        private string[] _androidProjectPackages;

        private Metadata _metadata { get; }

        public void AddMetadata(string section, string key, object value) => _metadata.AddMetadata(section,key,value);

        public void AddMetadata(string section, IDictionary<string, object> metadata) => _metadata.AddMetadata(section, metadata);

        public IDictionary<string, object> GetMetadata(string section) => _metadata.GetMetadata(section);

        public object GetMetadata(string section, string key) => _metadata.GetMetadata(section,key);

        public void ClearMetadata(string section) => _metadata.ClearMetadata(section);

        public void ClearMetadata(string section, string key) => _metadata.ClearMetadata(section, key);

        internal int PayloadVersion = 4;

        public string ApiKey { get; set; }

        private List<Breadcrumb> _breadcrumbs;

        public ReadOnlyCollection<IBreadcrumb> Breadcrumbs { get; }

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

        public void SetUser(string id, string email, string name)
        {
            _user = new User(id, email, name );
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

        internal void RedactMetadata(Configuration config)
        {
            foreach (var section in _metadata.Payload)
            {
                var theSection = (IDictionary<string, object>)section.Value;
                foreach (var key in theSection.Keys.ToList())
                {
                    if (config.KeyIsRedacted(key))
                    {
                        theSection[key] = "[REDACTED]";
                    }
                }
            }
            foreach (var crumb in _breadcrumbs)
            {
                foreach (var key in crumb.Metadata.Keys.ToList())
                {
                    if (config.KeyIsRedacted(key))
                    {
                        crumb.Metadata[key] = "[REDACTED]";
                    }
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
