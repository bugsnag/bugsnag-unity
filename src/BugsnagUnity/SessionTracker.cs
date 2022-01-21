using BugsnagUnity.Payload;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BugsnagUnity.Tests")]

namespace BugsnagUnity
{
    public interface ISessionTracker
    {
        void StartSession();

        void PauseSession();

        bool ResumeSession();

        Session CurrentSession { get; }

        void AddException(Report report);

        void StartManagedSession();
    }

    class SessionTracker : ISessionTracker
    {
        Client Client { get; }

        Session _currentSession;

        public Session CurrentSession
        {
            get
            {
                return GetCurrentSession();
            }
            private set => _currentSession = value;
        }

        private Session GetCurrentSession()
        {
            if (ShouldManageSessions())
            {
                var session = _currentSession;

                if (session != null && !session.Stopped)
                {
                    return session?.Copy();
                }
                return null;
            }
            else
            {
                return Client.NativeClient.GetCurrentSession();
            }
            
        }

        internal SessionTracker(Client client)
        {
            Client = client;
        }


        public void StartSession()
        {
            if (ShouldManageSessions())
            {
                StartManagedSession();
            }
            else
            {
                Client.NativeClient.StartSession();
            }
        }

        public void StartManagedSession()
        {
            var session = new Session();

            var app = new App(Client.Configuration);
            Client.NativeClient.PopulateApp(app);
            session.App = app;

            var device = new Device(Client.Configuration);
            Client.NativeClient.PopulateDevice(device);
            session.Device = device;

            session.User = Client.GetUser().Clone();

            foreach (var sessionCallback in Client.Configuration.GetOnSessionCallbacks())
            {
                try {
                    if (!sessionCallback.Invoke(session))
                    {
                        return;
                    }
                } catch {
                    // If the callback causes an exception, ignore it and execute the next one
                }
            }

            if (Client.Configuration.Endpoints.IsValid)
            {
                var payload = new SessionReport(Client.Configuration, app, device, Client.GetUser().Clone(), session);
                FileManager.CacheSession(payload);
                Client.Send(payload);
            }
            else
            {
                UnityEngine.Debug.LogWarning("Invalid configuration. Configuration.Endpoints is not correctly configured, no sessions will be sent.");
            }

            CurrentSession = session;
        }

        public void PauseSession()
        {
            if (ShouldManageSessions())
            {
                PauseManagedSession();
            }
            else
            {
                Client.NativeClient.PauseSession();
            }
        }

        private void PauseManagedSession()
        {
            var session = _currentSession;
            if (session != null)
            {
                session.Stopped = true;
            }
        }

        public bool ResumeSession()
        {
            if (ShouldManageSessions())
            {
                return ResumeManagedSession();
            }
            else
            {
                return Client.NativeClient.ResumeSession();
            }
             
        }

        private bool ResumeManagedSession()
        {
            var session = _currentSession;
            bool resumed;
            if (session == null)
            {
                StartSession();
                resumed = false;
            }
            else
            {
                resumed = session.Stopped;
                session.Stopped = false;
            }
            return resumed;
        }

        public void AddException(Report report)
        {
            if (ShouldManageSessions())
            {
                _currentSession?.AddException(report);
            }
            else
            {
                Client.NativeClient.UpdateSession(report.Session);
            }
        }

        private bool ShouldManageSessions()
        {
            return Client.IsUsingFallback();
        }
    }
}
