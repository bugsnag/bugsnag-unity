using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity.Payload
{
  class Event : Dictionary<string, object>
  {
    HandledState _handledState;

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
      if (session != null)
      {
        if (handledState.Handled)
        {
          session.Events.IncrementHandledCount();
        }
        else
        {
          session.Events.IncrementUnhandledCount();
        }
      }
      this.AddToPayload("user", user);
    }

    internal Metadata Metadata { get; }

    internal LogType? LogType { get; }

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

    internal App App
    {
      get { return this.Get("app") as App; }
    }

    internal IEnumerable<Breadcrumb> Breadcrumbs
    {
      get { return this.Get("breadcrumbs") as IEnumerable<Breadcrumb>; }
    }

    internal string Context
    {
      get => this.Get("context") as string;
      set => this.AddToPayload("context", value);
    }

    internal Device Device => this.Get("device") as Device;

    internal Exception[] Exceptions => this.Get("exceptions") as Exception[];

    internal string GroupingHash
    {
      get => this.Get("groupingHash") as string;
      set => this.AddToPayload("groupingHash", value);
    }

    internal Severity Severity
    {
      set => HandledState = HandledState.ForCallbackSpecifiedSeverity(value, _handledState);
      get => _handledState.Severity;
    }

    internal User User
    {
      get { return this.Get("user") as User; }
      set { this.AddToPayload("user", value); }
    }

    HandledState HandledState
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
