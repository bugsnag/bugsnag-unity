using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity.Payload
{
    class SessionReport : Dictionary<string, object>, IPayload
    {
        Configuration Configuration { get; }

        public Uri Endpoint => Configuration.Endpoints.Session;

        public KeyValuePair<string, string>[] Headers { get; }

        public string Id { get; set; }

        public PayloadType PayloadType { get => PayloadType.Session; }

        public SessionReport(Configuration configuration, App app, Device device, User user, Payload.Session session)
        {
            Configuration = configuration;
            Headers = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("Bugsnag-Api-Key", Configuration.ApiKey),
                new KeyValuePair<string, string>("Bugsnag-Payload-Version", Configuration.SessionPayloadVersion),
            };
            this.AddToPayload("notifier", NotifierInfo.Instance);
            this.AddToPayload("app", app.Payload);
            this.AddToPayload("device", device.Payload);
            this.AddToPayload("sessions", new Session[] { new Session(user, session) });
            Id = session.Id;
        }

        public SessionReport(Configuration configuration, Dictionary<string,object> cachedReport)
        {
            Configuration = configuration;
            Headers = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("Bugsnag-Api-Key", Configuration.ApiKey),
                new KeyValuePair<string, string>("Bugsnag-Payload-Version", Configuration.SessionPayloadVersion),
            };
            this.AddToPayload("notifier", cachedReport.Get("notifier"));
            this.AddToPayload("app", cachedReport.Get("app"));
            this.AddToPayload("device", cachedReport.Get("device"));
            this.AddToPayload("sessions", cachedReport.Get("sessions"));
            Debug.Log("SettingSessionId");
            var sessions = (Dictionary<string, object>[])this.Get("sessions");
            Debug.Log("Sessions count: " + sessions.Length);
            Id = sessions[0].Get("id").ToString();
            Debug.Log("SettingSessionId done");
        }

        class Session : Dictionary<string, object>
        {
            public Session(User user, Payload.Session session)
            {
                this.AddToPayload("id", session.Id);
                this.AddToPayload("startedAt", session.StartedAt);
                this.AddToPayload("user", user.Payload);
            }
        }
    }
}
