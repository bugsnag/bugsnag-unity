using System;
using System.Collections.Generic;
using System.Linq;

namespace Bugsnag.Unity.Payload
{
  class Report : Dictionary<string, object>, IPayload
  {
    IConfiguration Configuration { get; }

    public Uri Endpoint => Configuration.Endpoint;

    public KeyValuePair<string, string>[] Headers { get; }

    internal Report(IConfiguration configuration, System.Exception exception, HandledState handledState, Breadcrumb[] breadcrumbs, Session session)
      : this(configuration, new Exceptions(exception).ToArray(), handledState, breadcrumbs, session)
    {
    }

    internal Report(IConfiguration configuration, UnityLogMessage logMessage, HandledState handledState, Breadcrumb[] breadcrumbs, Session session)
      : this(configuration, new UnityLogExceptions(logMessage).ToArray(), handledState, breadcrumbs, session)
    {

    }

    internal Report(IConfiguration configuration, Exception[] exceptions, HandledState handledState, Breadcrumb[] breadcrumbs, Session session)
    {
      Configuration = configuration;
      Headers = new KeyValuePair<string, string>[] {
        new KeyValuePair<string, string>("Bugsnag-Api-Key", Configuration.ApiKey),
        new KeyValuePair<string, string>("Bugsnag-Payload-Version", Configuration.PayloadVersion),
      };
      Event = new Event(new App(Configuration), new Device(), exceptions.ToArray(), handledState, breadcrumbs, session);
      this.AddToPayload("apiKey", configuration.ApiKey);
      this.AddToPayload("notifier", NotifierInfo.Instance);
      this.AddToPayload("events", new Event[] { Event });
    }

    internal Event Event { get; }
  }
}
