using Bugsnag.Unity.Payload;

namespace Bugsnag.Unity
{
  public class SessionTracker
  {
    private Client Client { get; }

    public Session CurrentSession { get; private set; }

    public SessionTracker(Client client)
    {
      Client = client;
    }

    public void StartSession()
    {
      var session = new Session();

      CurrentSession = session;

      var payload = new SessionReport(Client.Configuration, Client.User, session);

      ThreadQueueDelivery.Instance.Send(payload);
    }
  }
}
