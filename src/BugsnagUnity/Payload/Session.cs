using System;
using System.Collections.Generic;
#nullable enable

namespace BugsnagUnity.Payload
{
    public class Session : PayloadContainer, ISession
    {

        private const string ID_KEY = "id";
        private const string STARTED_AT_KEY = "startedAt";
        private const string EVENTS_KEY = "events";

        public string? Id
        {
            get => (string?)Get(ID_KEY);
            set => Add(ID_KEY, value);
        }

        public DateTime? StartedAt
        {
            get => (DateTime?)Get(STARTED_AT_KEY);
            set => Add(STARTED_AT_KEY, value);
        }

        public App? App { get; set; }

        public Device? Device { get; set; }

        internal User? User { get; set; }

        public User GetUser() => User;

        public void SetUser(string id, string email, string name)
        {
            User = new User(id,name,email);
        }

        internal int HandledCount()
        {
            return this.Events.Handled;
        }

        internal int UnhandledCount()
        {
            return this.Events.Unhandled;
        }

        internal SessionEvents Events
        {
            get => (SessionEvents)Get(EVENTS_KEY);
            set => Add(EVENTS_KEY, value);
        }

        internal bool Stopped { get; set; }

        internal Session() : this(DateTime.UtcNow, 0, 0)
        {

        }

        internal Session(DateTime? startedAt, int handled, int unhandled)
        {
            Id = Guid.NewGuid().ToString();
            StartedAt = startedAt;
            Events = new SessionEvents(handled, unhandled);
        }

        internal Session(string providedGuid, DateTime startedAt, int handled, int unhandled)
        {
            Id = providedGuid;
            StartedAt = startedAt;
            Events = new SessionEvents(handled, unhandled);
        }

        internal void AddException(Report report)
        {
            if (report.IsHandled)
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
                Id = Id
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
