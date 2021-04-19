using System.Collections.Generic;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
  public static class Bugsnag
  {
    static object _clientLock = new object();

    public static IClient Start(string apiKey)
    {
      return Start(new Configuration(apiKey, true));
    }

    public static IClient Start(IConfiguration configuration)
    {
      lock (_clientLock)
      {
        if (InternalClient == null)
        {
          var nativeClient = new NativeClient(configuration);
          InternalClient = new Client(nativeClient);
        }
      }

      return Client;
    }

    static Client InternalClient { get; set; }

    public static IClient Client => InternalClient;

    public static IConfiguration Configuration => Client.Configuration;

    public static IBreadcrumbs Breadcrumbs => Client.Breadcrumbs;

    public static ISessionTracker SessionTracking => Client.SessionTracking;

    public static User User => Client.User;

    public static void Send(IPayload payload) => Client.Send(payload);

    public static Metadata Metadata => Client.Metadata;

    public static void BeforeNotify(Middleware middleware) => Client.BeforeNotify(middleware);

    public static void Notify(System.Exception exception) => InternalClient.Notify(exception, 3);

    public static void Notify(System.Exception exception, Middleware callback) => InternalClient.Notify(exception, callback, 3);

    public static void Notify(System.Exception exception, Severity severity) => InternalClient.Notify(exception, severity, 3);

    public static void Notify(System.Exception exception, Severity severity, Middleware callback) => InternalClient.Notify(exception, severity, callback, 3);

    public static void LeaveBreadcrumb(string message) => InternalClient.Breadcrumbs.Leave(message);

    public static void LeaveBreadcrumb(string message, BreadcrumbType type, IDictionary<string, string> metadata) => InternalClient.Breadcrumbs.Leave(message, type, metadata);

    public static void LeaveBreadcrumb(Breadcrumb breadcrumb) => InternalClient.Breadcrumbs.Leave(breadcrumb);

    public static void SetUser(string id, string email, string name) {
      User.Id = id;
      User.Email = email;
      User.Name = name;
    }

    public static void StartSession() => InternalClient.SessionTracking.StartSession();

    public static void StopSession() => InternalClient.SessionTracking.StopSession();

    public static bool ResumeSession() => InternalClient.SessionTracking.ResumeSession();

    /// <summary>
    /// Used to signal to the Bugsnag client that the focused state of the
    /// application has changed. This is used for session tracking and also
    /// the tracking of in foreground time for the application.
    /// </summary>
    /// <param name="inFocus"></param>
    public static void SetApplicationState(bool inFocus) => Client.SetApplicationState(inFocus);
  }
}
