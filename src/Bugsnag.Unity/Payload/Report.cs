using System;
using System.Collections.Generic;

namespace Bugsnag.Unity.Payload
{
  public class Report : Dictionary<string, object>, IPayload
  {
    IConfiguration Configuration { get; }

    public Uri Endpoint => Configuration.Endpoint;

    public KeyValuePair<string, string>[] Headers { get; }

    public bool Ignored { get; private set; }

    internal Report(IConfiguration configuration, Event @event)
    {
      Ignored = false;
      Configuration = configuration;
      Headers = new[] {
        new KeyValuePair<string, string>("Bugsnag-Api-Key", Configuration.ApiKey),
        new KeyValuePair<string, string>("Bugsnag-Payload-Version", Configuration.PayloadVersion),
      };
      Event = @event;
      this.AddToPayload("apiKey", configuration.ApiKey);
      this.AddToPayload("notifier", NotifierInfo.Instance);
      this.AddToPayload("events", new[] { Event });
    }

    public Event Event { get; }

    public void Ignore()
    {
      Ignored = true;
    }
  }
}
