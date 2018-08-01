using BugsnagUnity.Payload;

namespace BugsnagUnity
{
  public interface ISessionTracker
  {
    void StartSession();

    Session CurrentSession { get; }
  }

  class SessionTracker : ISessionTracker
  {
    private IClient Client { get; }

    private Session _currentSession;

    public Session CurrentSession
    {
      get => _currentSession.Copy();
      private set => _currentSession = value;
    }

    internal SessionTracker(IClient client)
    {
      Client = client;
    }

    public void StartSession()
    {
      var session = new Session();

      CurrentSession = session;

      var payload = new SessionReport(Client.Configuration, Client.User, session);

      Client.Send(payload);
    }
  }
}
