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

    public Session CurrentSession { get; private set; }

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
