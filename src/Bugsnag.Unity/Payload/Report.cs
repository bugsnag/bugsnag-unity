using System;
using System.Collections.Generic;
using System.Linq;

namespace Bugsnag.Unity.Payload
{
  public class Report : Dictionary<string, object>, IPayload
  {
    Configuration Configuration { get; }

    public Uri Endpoint => Configuration.Endpoint;

    public KeyValuePair<string, string>[] Headers { get; }

    public Report(Configuration configuration, System.Exception exception, HandledState handledState, IEnumerable<Breadcrumb> breadcrumbs, Session session)
      : this(configuration, new Exceptions(exception), handledState, breadcrumbs, session)
    {
    }

    public Report(Configuration configuration, UnityLogMessage logMessage, HandledState handledState, IEnumerable<Breadcrumb> breadcrumbs, Session session)
      : this(configuration, new UnityLogExceptions(logMessage), handledState, breadcrumbs, session)
    {

    }

    public Report(Configuration configuration, IEnumerable<Exception> exceptions, HandledState handledState, IEnumerable<Breadcrumb> breadcrumbs, Session session)
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

    public Event Event { get; }
  }
}
