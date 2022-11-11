using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity.Payload
{
    class SessionReport : Dictionary<string, object>, IPayload
    {

        public Uri Endpoint { get; set; }

        public KeyValuePair<string, string>[] Headers { get; set; }

        public string Id { get; set; }

        public PayloadType PayloadType { get => PayloadType.Session; }

        private void SetRequestInfo(Configuration configuration)
        {
            Endpoint = configuration.Endpoints.Session;
            Headers = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("Bugsnag-Api-Key", configuration.ApiKey),
                new KeyValuePair<string, string>("Bugsnag-Payload-Version", configuration.SessionPayloadVersion),
            };
        }

        public SessionReport(Configuration configuration, App app, Device device, User user, Session session)
        {
            SetRequestInfo(configuration);
            this.AddToPayload("notifier", NotifierInfo.Instance);
            this.AddToPayload("app", app.Payload);
            this.AddToPayload("device", device.Payload);
            var sessionObject = new Dictionary<string,object>();
            sessionObject.AddToPayload("id", session.Id);
            sessionObject.AddToPayload("startedAt", session.StartedAt);
            sessionObject.AddToPayload("user", user.Payload);
            this.AddToPayload("sessions", new [] { sessionObject });
            Id = session.Id;
        }

        public SessionReport(Configuration configuration, Dictionary<string, object> cachedReport)
        {
            SetRequestInfo(configuration);
            this.AddToPayload("notifier", cachedReport.Get("notifier"));
            this.AddToPayload("app", cachedReport.Get("app"));
            this.AddToPayload("device", cachedReport.Get("device"));
            this.AddToPayload("sessions", cachedReport.Get("sessions"));
            Id = cachedReport["id"].ToString();
        }

        public Dictionary<string, object> GetSerialisablePayload()
        {
            var serialisableSessionReport = new Dictionary<string, object>
                {
                    { "id", Id },
                    { "app", this["app"] },
                    { "device", this["device"] },
                    { "notifier", this["notifier"] },
                    { "sessions", this["sessions"]}
                };
            return serialisableSessionReport;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
