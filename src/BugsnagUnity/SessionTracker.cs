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
  }

  class SessionTracker : ISessionTracker
  {
    Client Client { get; }

    Session _currentSession;

    public Session CurrentSession
    {
      get {
        var session = _currentSession;

        if (session != null && !session.Stopped) {
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
      var session = new Session();

      CurrentSession = session;

      if (Client == null) {
        return;
      }

      var app = new App(Client.Configuration);
      Client.NativeClient.PopulateApp(app);
      var device = new Device();
      Client.NativeClient.PopulateDevice(device);

      var payload = new SessionReport(Client.Configuration, app, device, Client.User, session);

      Client.Send(payload);
    }

    public void StopSession()
    {
      var session = _currentSession;

      if (session != null) {
        session.Stopped = true;
      }
    }

    public bool ResumeSession()
    {
      var session = _currentSession;
      var resumed = false;

      if (session == null) {
        StartSession();
        resumed = false;
      } else {
        resumed = session.Stopped;
        session.Stopped = false;
      }
      return resumed;
    }
    
    public void AddException(Report report)
    {
      CurrentSession?.AddException(report);
    }
  }
}
