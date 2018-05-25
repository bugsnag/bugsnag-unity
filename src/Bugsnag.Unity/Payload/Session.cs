using System;
using System.Collections.Generic;

namespace Bugsnag.Unity.Payload
{
  public class Session : Dictionary<string, object>
  {
    public Guid Id { get; }

    public DateTime StartedAt { get; }

    private SessionEvents Events { get; }

    public Session() : this(DateTime.UtcNow, 0, 0)
    {

    }

    public Session(DateTime startedAt, int handled, int unhandled)
    {
      this.AddToPayload("id", Id = Guid.NewGuid());
      this.AddToPayload("startedAt", StartedAt = startedAt);
      this.AddToPayload("events", Events = new SessionEvents(handled, unhandled));
    }

    public void AddException(Report report)
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
  }

  public class SessionEvents : Dictionary<string, int>
  {
    private readonly object _handledLock = new object();
    private readonly object _unhandledLock = new object();

    public SessionEvents(int handled, int unhandled)
    {
      this.AddToPayload("handled", handled);
      this.AddToPayload("unhandled", unhandled);
    }

    public void IncrementHandledCount()
    {
      lock (_handledLock)
      {
        this["handled"]++;
      }
    }

    public void IncrementUnhandledCount()
    {
      lock (_unhandledLock)
      {
        this["unhandled"]++;
      }
    }
  }
}
