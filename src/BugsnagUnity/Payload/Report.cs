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

        private string _payloadVersion;

        internal Report(Configuration configuration, Event @event)
        {
            Id = Guid.NewGuid().ToString();
            Ignored = false;
            Endpoint = configuration.Endpoints.Notify;
            _payloadVersion = configuration.PayloadVersion;
            Headers = new[] {
                new KeyValuePair<string, string>("Bugsnag-Api-Key", @event.ApiKey),
                new KeyValuePair<string, string>("Bugsnag-Payload-Version", _payloadVersion),
            };
            _event = @event;
            this.AddToPayload("apiKey", @event.ApiKey);
            this.AddToPayload("notifier", NotifierInfo.Instance);
            this.AddToPayload("events", new[] { _event.GetEventPayload() });
        }

        internal Report(Configuration configuration, Dictionary<string, object> data)
        {
            Id = data["id"].ToString();
            Endpoint = configuration.Endpoints.Notify;
            var apiKey = data["apiKey"].ToString();
            Headers = new[] {
                new KeyValuePair<string, string>("Bugsnag-Api-Key", apiKey),
                new KeyValuePair<string, string>("Bugsnag-Payload-Version", data["payloadVersion"].ToString()),
            };
            this.AddToPayload("apiKey", apiKey);
            this.AddToPayload("notifier", data["notifier"]);
            this.AddToPayload("events", new[] { data["event"] });
        }

        internal Dictionary<string, object> GetSerialisableDictionary()
        {
            var serialisableReport = new Dictionary<string, object>();
            serialisableReport["id"] = Id;
            serialisableReport["apiKey"] = _event.ApiKey;
            serialisableReport["notifier"] = NotifierInfo.Instance;
            serialisableReport["event"] = _event.GetEventPayload();
            serialisableReport["payloadVersion"] = _payloadVersion;
            return serialisableReport;
        }

        private Event _event;

        internal Session Session => _event.Session;

        internal bool IsHandled => _event.IsHandled;

        internal string Context => _event.Context;

        internal List<IError> Exceptions => _event.Errors;

        internal HandledState OriginalSeverity => _event.OriginalSeverity;

        public PayloadType PayloadType => PayloadType.Event;

        public void Ignore()
        {
            Ignored = true;
        }

    }
}
