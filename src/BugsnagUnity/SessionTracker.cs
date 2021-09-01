using BugsnagUnity.Payload;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BugsnagUnity.Tests")]

namespace BugsnagUnity
{
    public interface ISessionTracker
    {
        void StartSession();

        void StopSession();

        bool ResumeSession();

        Session CurrentSession { get; }

        void AddException(Report report);

        void StartTrackingFallbackSession();
    }

    class SessionTracker : ISessionTracker
    {
        Client Client { get; }

        Session _currentSession;

        public Session CurrentSession
        {
            get
            {
                var session = _currentSession;

                if (session != null && !session.Stopped)
                {
                    return session?.Copy();
                }
                return null;
            }
            private set => _currentSession = value;
        }

        internal SessionTracker(Client client)
        {
            Client = client;
        }


        public void StartSession()
        {
            if (ShouldManageSessions())
            {
                StartTrackingFallbackSession();
            }
            else
            {
                Client.NativeClient.StartSession();
            }
        }

        public void StartTrackingFallbackSession()
        {
            var session = new Session();

            CurrentSession = session;

            var app = new App(Client.Configuration);
            Client.NativeClient.PopulateApp(app);
            var device = new Device();
            Client.NativeClient.PopulateDevice(device);
            device.AddRuntimeVersions(Client.Configuration);

            if (Client.Configuration.Endpoints.IsValid)
            {
                var payload = new SessionReport(Client.Configuration, app, device, Client.User, session);
                Client.Send(payload);
            }
            else
            {
                UnityEngine.Debug.LogWarning("Invalid configuration. Configuration.Endpoints is not correctly configured, no sessions will be sent.");
            }
        }

        public void StopSession()
        {
            if (ShouldManageSessions())
            {
                PauseFallbackSession();
            }
            else
            {
                Client.NativeClient.PauseSession();
            }
        }

        private void PauseFallbackSession()
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
                return ResumeFallbackSession();
            }
            else
            {
                return Client.NativeClient.ResumeSession();
            }
             
        }

        private bool ResumeFallbackSession()
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
            _currentSession?.AddException(report);
        }

        private bool ShouldManageSessions()
        {
            return Client.IsUsingFallback();
        }
    }
}
