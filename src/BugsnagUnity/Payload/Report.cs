using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace BugsnagUnity.Payload
{
    public class Report : Dictionary<string, object>, IPayload
    {

        public Uri Endpoint { get; set; }

        public KeyValuePair<string, string>[] Headers { get; set; }

        internal bool Ignored { get; private set; }

        public string Id { get; set; }

        internal Report(Configuration configuration, Event @event)
        {
            Id = Guid.NewGuid().ToString();
            Ignored = false;
            Endpoint = configuration.Endpoints.Notify;
            Headers = new[] {
                new KeyValuePair<string, string>("Bugsnag-Api-Key", @event.ApiKey),
                new KeyValuePair<string, string>("Bugsnag-Payload-Version", configuration.PayloadVersion),
            };
            Event = @event;
            this.AddToPayload("apiKey", @event.ApiKey);
            this.AddToPayload("notifier", NotifierInfo.Instance);
            this.AddToPayload("events", new[] { Event.GetEventPayload() });
        }

        internal Report(Configuration configuration, Dictionary<string, object> data)
        {
            Id = data["id"].ToString();
            Endpoint = configuration.Endpoints.Notify;
            var apiKey = data["apiKey"].ToString();
            Headers = new[] {
                new KeyValuePair<string, string>("Bugsnag-Api-Key", apiKey),
                new KeyValuePair<string, string>("Bugsnag-Payload-Version", configuration.PayloadVersion),
            };
            this.AddToPayload("apiKey", apiKey);
            this.AddToPayload("notifier", data["notifier"]);
            Event = new Event(data);
        }

        internal Dictionary<string, object> GetSerialisableEventReport()
        {
            var serialisableReport = new Dictionary<string, object>();
            serialisableReport["id"] = Id;
            serialisableReport["apiKey"] = Event.ApiKey;
            serialisableReport["notifier"] = NotifierInfo.Instance;
            serialisableReport["event"] = Event.GetEventPayload();
            return serialisableReport;
        }

        internal void ApplyEventPayload()
        {
            this.AddToPayload("events", new[] { Event.GetEventPayload() });
        }

        internal Event Event;

        internal Session Session => Event.Session;

        internal bool IsHandled => Event.IsHandled;

        internal string Context => Event.Context;

        internal List<IError> Exceptions => Event.Errors;

        internal HandledState OriginalSeverity => Event.OriginalSeverity;

        public PayloadType PayloadType => PayloadType.Event;

        public void Ignore()
        {
            Ignored = true;
        }

    }
}
