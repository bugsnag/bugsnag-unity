using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity.Payload
{
  public class Event : Dictionary<string, object>
  {
    private HandledState _handledState;

    internal HandledState OriginalSeverity { get; }

    internal Event(string context, Metadata metadata, App app, Device device, User user, Exception[] exceptions, HandledState handledState, Breadcrumb[] breadcrumbs, Session session, LogType? logType = null)
    {
      OriginalSeverity = handledState;
      Metadata = metadata;
      HandledState = handledState;
      LogType = logType;
      this.AddToPayload("context", context);
      this.AddToPayload("payloadVersion", 4);
      this.AddToPayload("exceptions", exceptions);
      this.AddToPayload("app", app);
      this.AddToPayload("device", device);
      this.AddToPayload("metaData", Metadata);
      this.AddToPayload("breadcrumbs", breadcrumbs);
      this.AddToPayload("session", session);
      this.AddToPayload("user", user);
    }

    public Metadata Metadata { get; }

    public LogType? LogType { get; }

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
      get => this.Get("context") as string;
      set => this.AddToPayload("context", value);
    }

    public string GroupingHash
    {
      get => this.Get("groupingHash") as string;
      set => this.AddToPayload("groupingHash", value);
    }

    public Severity Severity
    {
      set => HandledState = HandledState.ForCallbackSpecifiedSeverity(value, _handledState);
      get => _handledState.Severity;
    }

    public Exception[] Exceptions => this.Get("exceptions") as Exception[];

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
