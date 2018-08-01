using System;
using System.Collections.Generic;

namespace BugsnagUnity.Payload
{
  public class Session : Dictionary<string, object>
  {
    public Guid Id { get; }

    public DateTime StartedAt { get; }

    internal SessionEvents Events { get; }

    internal Session() : this(DateTime.UtcNow, 0, 0)
    {

    }

    internal Session(DateTime startedAt, int handled, int unhandled)
    {
      this.AddToPayload("id", Id = Guid.NewGuid());
      this.AddToPayload("startedAt", StartedAt = startedAt);
      this.AddToPayload("events", Events = new SessionEvents(handled, unhandled));
    }

    internal void AddException(Report report)
    {
      if (report.Event.IsHandled)
      {
        Events.IncrementHandledCount();
      }
      else
      {
        Events.IncrementUnhandledCount();
      }
    }

    internal Session Copy()
    {
      var copy = new Session(StartedAt, Events.Handled, Events.Unhandled)
      {
        ["id"] = Id
      };
      return copy;
    }
  }

  internal class SessionEvents : Dictionary<string, int>
  {
    private readonly object _handledLock = new object();
    private readonly object _unhandledLock = new object();

    internal SessionEvents(int handled, int unhandled)
    {
      this.AddToPayload("handled", handled);
      this.AddToPayload("unhandled", unhandled);
    }

    internal int Handled => this["handled"];
    internal int Unhandled => this["unhandled"];

    internal void IncrementHandledCount()
    {
      lock (_handledLock)
      {
        this["handled"]++;
      }
    }

    internal void IncrementUnhandledCount()
    {
      lock (_unhandledLock)
      {
        this["unhandled"]++;
      }
    }
  }
}
