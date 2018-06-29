using System.Collections.Generic;

namespace Bugsnag.Unity.Payload
{
  class Event : Dictionary<string, object>
  {
    private HandledState _handledState;

    internal Event(Metadata metadata, App app, Device device, User user, Exception[] exceptions, HandledState handledState, Breadcrumb[] breadcrumbs, Session session)
    {
      Metadata = metadata;
      HandledState = handledState;
      this.AddToPayload("payloadVersion", 4);
      this.AddToPayload("exceptions", exceptions);
      this.AddToPayload("app", app);
      this.AddToPayload("device", device);
      this.AddToPayload("metaData", Metadata);
      this.AddToPayload("breadcrumbs", breadcrumbs);
      this.AddToPayload("session", session);
      this.AddToPayload("user", user);
    }

    internal Metadata Metadata { get; }

    internal bool IsHandled
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

    internal string Context
    {
      get { return this.Get("context") as string; }
      set { this.AddToPayload("context", value); }
    }

    internal string GroupingHash
    {
      get { return this.Get("groupingHash") as string; }
      set { this.AddToPayload("groupingHash", value); }
    }

    internal Severity Severity
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
