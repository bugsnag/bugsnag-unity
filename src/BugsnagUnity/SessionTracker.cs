using BugsnagUnity.Payload;

namespace BugsnagUnity
{
  public interface ISessionTracker
  {
    void StartSession();

    Session CurrentSession { get; }

    void AddException(Report report);
  }

  class SessionTracker : ISessionTracker
  {
    Client Client { get; }

    Session _currentSession;

    public Session CurrentSession
    {
      get => _currentSession?.Copy();
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
      Client.NativeClient.SetSession(session);

      var app = new App(Client.Configuration);
      Client.NativeClient.PopulateApp(app);
      var device = new Device();
      Client.NativeClient.PopulateDevice(device);

      var payload = new SessionReport(Client.Configuration, app, device, Client.User, session);

      Client.Send(payload);
    }
    
    public void AddException(Report report)
    {
      _currentSession?.AddException(report);
    }
  }
}
