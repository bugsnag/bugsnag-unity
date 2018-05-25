using System.Collections.Generic;

namespace Bugsnag.Unity.Payload
{
  public class Event : Dictionary<string, object>
  {
    private HandledState _handledState;

    public Event(App app, Device device, IEnumerable<Exception> exceptions, HandledState handledState, IEnumerable<Breadcrumb> breadcrumbs, Session session)
    {
      Metadata = new Metadata();
      HandledState = handledState;
      this.AddToPayload("payloadVersion", 4);
      this.AddToPayload("exceptions", exceptions);
      this.AddToPayload("app", app);
      this.AddToPayload("device", device);
      this.AddToPayload("metaData", Metadata);
      this.AddToPayload("breadcrumbs", breadcrumbs);
      this.AddToPayload("session", session);
    }

    public Metadata Metadata { get; }

    public bool IsHandled
    {
      get
      {
        if (this.Get("unhandled") is bool unhandled)
        {
          return !unhandled;
        }

        return false;
      }
    }

    public string Context
    {
      get { return this.Get("context") as string; }
      set { this.AddToPayload("context", value); }
    }

    public string GroupingHash
    {
      get { return this.Get("groupingHash") as string; }
      set { this.AddToPayload("groupingHash", value); }
    }

    public Severity Severity
    {
      set
      {
        HandledState = HandledState.ForCallbackSpecifiedSeverity(value, _handledState);
      }
      get
      {
        return _handledState.Severity;
      }
    }

    private HandledState HandledState
    {
      set
      {
        _handledState = value;
        foreach (var item in value)
        {
          this[item.Key] = item.Value;
        }
      }
    }
  }
}
