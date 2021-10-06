using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity.Payload
{
    public class Event : Dictionary<string, object>
    {
        HandledState _handledState;

        internal HandledState OriginalSeverity { get; }

        internal Event(string context, Metadata metadata, AppWithState app, DeviceWithState device, User user, Exception[] exceptions, HandledState handledState, List<Breadcrumb> breadcrumbs, Session session, LogType? logType = null)
        {
            OriginalSeverity = handledState;
            Metadata = metadata;
            HandledState = handledState;
            LogType = logType;
            App = app;
            Device = device;
            Context = context;
            Exceptions = exceptions;
            Breadcrumbs = breadcrumbs;
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
            User = user;
        }
        internal void AddAndroidProjectPackagesToEvent(string[] packages)
        {
            this.AddToPayload("projectPackages", packages);
        }

        public Metadata Metadata { get; }

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

        public Exception[] Exceptions { get; }

        internal string GroupingHash
        {
            get => this.Get("groupingHash") as string;
            set => this.AddToPayload("groupingHash", value);
        }

        internal Severity Severity
        {
            set => HandledState = HandledState.ForCallbackSpecifiedSeverity(value, _handledState);
            get => _handledState.Severity;
        }

        public User User;

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
            this.AddToPayload("context", Context);
            this.AddToPayload("payloadVersion", 4);
            this.AddToPayload("user", User.Payload);
            this.AddToPayload("exceptions", Exceptions);
            this.AddToPayload("metaData", Metadata);
            this.AddToPayload("breadcrumbs", Breadcrumbs);
            if (Session != null)
            {
                this.AddToPayload("session", Session.Payload);
            }
        }
    }
}
