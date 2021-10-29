using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity.Payload
{
    public class Report : Dictionary<string, object>, IPayload
    {

        public Uri Endpoint { get; }

        public KeyValuePair<string, string>[] Headers { get; }

        internal bool Ignored { get; private set; }

        internal Report(Configuration configuration, Event @event)
        {
            Ignored = false;
            Endpoint = configuration.Endpoints.Notify;
            Headers = new[] {
                new KeyValuePair<string, string>("Bugsnag-Api-Key", @event.ApiKey),
                new KeyValuePair<string, string>("Bugsnag-Payload-Version", configuration.PayloadVersion),
            };
            _event = @event;
            this.AddToPayload("apiKey", @event.ApiKey);
            this.AddToPayload("notifier", NotifierInfo.Instance);
            this.AddToPayload("events", new[] { _event.GetEventPayload() });
        }

        private Event _event;

        internal Session Session => _event.Session;

        internal bool IsHandled => _event.IsHandled;

        internal string Context => _event.Context;

        internal List<Exception> Exceptions => _event.Exceptions;

        internal HandledState OriginalSeverity => _event.OriginalSeverity;

        public void Ignore()
        {
            Ignored = true;
        }

    }
}
