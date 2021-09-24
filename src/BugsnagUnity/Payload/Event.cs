using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity.Payload
{
    class Event : Dictionary<string, object>
    {
        HandledState _handledState;

        internal HandledState OriginalSeverity { get; }

        internal Event(string context, Metadata metadata, AppWithState app, DeviceWithState device, User user, Exception[] exceptions, HandledState handledState, Breadcrumb[] breadcrumbs, Session session, LogType? logType = null)
        {
            OriginalSeverity = handledState;
            Metadata = metadata;
            HandledState = handledState;
            LogType = logType;
            this.AddToPayload("context", context);
            this.AddToPayload("payloadVersion", 4);
            this.AddToPayload("exceptions", exceptions);
            this.AddToPayload("app", app.Payload);
            this.AddToPayload("device", device.Payload);
            this.AddToPayload("metaData", Metadata);
            this.AddToPayload("breadcrumbs", breadcrumbs);
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
            }
            this.AddToPayload("session", session);
            Session = session;
            this.AddToPayload("user", user);
        }
        internal void AddAndroidProjectPackagesToEvent(string[] packages)
        {
            this.AddToPayload("projectPackages", packages);
        }

        internal Metadata Metadata { get; }

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

        internal App App
        {
            get { return this.Get("app") as AppWithState; }
        }

        internal IEnumerable<Breadcrumb> Breadcrumbs
        {
            get { return this.Get("breadcrumbs") as IEnumerable<Breadcrumb>; }
        }

        internal string Context
        {
            get => this.Get("context") as string;
            set => this.AddToPayload("context", value);
        }

        internal Device Device => this.Get("device") as DeviceWithState;

        internal Exception[] Exceptions => this.Get("exceptions") as Exception[];

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

        internal User User
        {
            get { return this.Get("user") as User; }
            set { this.AddToPayload("user", value); }
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
    }
}
