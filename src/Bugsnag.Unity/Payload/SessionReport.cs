using System;
using System.Collections.Generic;

namespace Bugsnag.Unity.Payload
{
  class SessionReport : Dictionary<string, object>, IPayload
  {
    IConfiguration Configuration { get; }

    public Uri Endpoint => Configuration.SessionEndpoint;

    public KeyValuePair<string, string>[] Headers { get; }

    public SessionReport(IConfiguration configuration, User user, Payload.Session session)
    {
      Configuration = configuration;
      Headers = new KeyValuePair<string, string>[] {
        new KeyValuePair<string, string>("Bugsnag-Api-Key", Configuration.ApiKey),
        new KeyValuePair<string, string>("Bugsnag-Payload-Version", Configuration.SessionPayloadVersion),
      };
      this.AddToPayload("notifier", NotifierInfo.Instance);
      this.AddToPayload("app", new App(configuration));
      this.AddToPayload("device", new Device());
      this.AddToPayload("sessions", new Session[] { new Session(user, session) });
    }

    class Session : Dictionary<string, object>
    {
      public Session(User user, Payload.Session session)
      {
        this.AddToPayload("id", session.Id);
        this.AddToPayload("startedAt", session.StartedAt);
        this.AddToPayload("user", user);
      }
    }
  }
}
