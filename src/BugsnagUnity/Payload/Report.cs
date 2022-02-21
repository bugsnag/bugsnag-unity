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
            _event = @event;
            this.AddToPayload("apiKey", @event.ApiKey);
            this.AddToPayload("notifier", NotifierInfo.Instance);
            this.AddToPayload("events", new[] { _event.GetEventPayload() });
        }

        internal string GetSerialisedReport()
        {
            var serialisableReport = new Dictionary<string, object>();
            serialisableReport["id"] = Id;
            serialisableReport["apiKey"] = _event.ApiKey;
            serialisableReport["notifier"] = NotifierInfo.Instance;
            serialisableReport["event"] = _event.GetEventPayload();
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
            {
                SimpleJson.SerializeObject(serialisableReport, writer);
                writer.Flush();
                stream.Position = 0;
                return reader.ReadToEnd();
            }
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
