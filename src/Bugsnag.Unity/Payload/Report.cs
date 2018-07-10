using System;
using System.Collections.Generic;

namespace Bugsnag.Unity.Payload
{
  class Report : Dictionary<string, object>, IPayload
  {
    IConfiguration Configuration { get; }

    public Uri Endpoint => Configuration.Endpoint;

    public KeyValuePair<string, string>[] Headers { get; }

    internal Report(IConfiguration configuration, Event @event)
    {
      Configuration = configuration;
      Headers = new KeyValuePair<string, string>[] {
        new KeyValuePair<string, string>("Bugsnag-Api-Key", Configuration.ApiKey),
        new KeyValuePair<string, string>("Bugsnag-Payload-Version", Configuration.PayloadVersion),
      };
      Event = @event;
      this.AddToPayload("apiKey", configuration.ApiKey);
      this.AddToPayload("notifier", NotifierInfo.Instance);
      this.AddToPayload("events", new Event[] { Event });
    }

    internal Event Event { get; }
  }
}
