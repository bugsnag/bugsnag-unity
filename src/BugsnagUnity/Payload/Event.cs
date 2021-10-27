using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BugsnagUnity.Payload
{
    public class Event : Dictionary<string, object>, IMetadataEditor, IUserEditor
    {
        HandledState _handledState;

        internal HandledState OriginalSeverity { get; }

        internal Event(string context, Metadata metadata, AppWithState app, DeviceWithState device, User user, Exception[] exceptions, HandledState handledState, List<Breadcrumb> breadcrumbs, Session session, LogType? logType = null)
        {
            OriginalSeverity = handledState;
            _metadata = metadata;
            HandledState = handledState;
            LogType = logType;
            App = app;
            Device = device;
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
            this.AddToPayload("projectPackages", packages);
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
                if (this.Get("unhandled") is bool unhandled)
                {
                    return !unhandled;
                }

                return false;
            }
        }

        public AppWithState App { get; }

        public string Context;

        public DeviceWithState Device { get; }

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
                    this[item.Key] = item.Value;
                }
            }
        }

        internal void PreparePayload()
        {
            this.AddToPayload("app", App.Payload);
            this.AddToPayload("device", Device.Payload);
            this.AddToPayload("metaData", _metadata.Payload);
            this.AddToPayload("user", _user.Payload);
            this.AddToPayload("context", Context);
            this.AddToPayload("groupingHash", GroupingHash);
            this.AddToPayload("payloadVersion", 4);
            this.AddToPayload("exceptions", Exceptions);
            var breadcrumbPayloads = new List<Dictionary<string, object>>();
            foreach (var crumb in Breadcrumbs)
            {
                breadcrumbPayloads.Add(crumb.Payload);
            }
            this.AddToPayload("breadcrumbs", breadcrumbPayloads);
            if (Session != null)
            {
                this.AddToPayload("session", Session.Payload);
            }
        }

    }
}
