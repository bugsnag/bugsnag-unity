using UnityEngine;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
  public static class Bugsnag
  {
    static object _clientLock = new object();

    public static IClient Init(string apiKey)
    {
      lock (_clientLock)
      {
        if (Client == null)
        {
          switch (Application.platform)
          {
            case RuntimePlatform.tvOS:
            case RuntimePlatform.IPhonePlayer:
              Client = new Client(new CocoaClient(new iOSConfiguration(apiKey)));
              break;
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
              Client = new Client(new CocoaClient(new MacOSConfiguration(apiKey)));
              break;
            case RuntimePlatform.Android:
              Client = new Client(new AndroidClient(new AndroidConfiguration(apiKey)));
              break;
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
              Client = new Client(new WindowsClient(new Configuration(apiKey)));
              break;
            default:
              Client = new Client(new NativeClient(new Configuration(apiKey)));
              break;
          }
        }
      }

      return Client;
    }

    public static IClient Client { get; private set; }

    public static IConfiguration Configuration => Client.Configuration;

    public static IBreadcrumbs Breadcrumbs => Client.Breadcrumbs;

    public static ISessionTracker SessionTracking => Client.SessionTracking;

    public static User User => Client.User;

    public static void Send(IPayload payload) => Client.Send(payload);

    public static Metadata Metadata => Client.Metadata;

    public static void BeforeNotify(Middleware middleware) => Client.BeforeNotify(middleware);

    public static void Notify(System.Exception exception) => Client.Notify(exception);

    public static void Notify(System.Exception exception, Middleware callback) => Client.Notify(exception, callback);

    public static void Notify(System.Exception exception, Severity severity) => Client.Notify(exception, severity);

    public static void Notify(System.Exception exception, Severity severity, Middleware callback) => Client.Notify(exception, severity, callback);

    /// <summary>
    /// Used to signal to the Bugsnag client that the focused state of the
    /// application has changed. This is used for session tracking and also
    /// the tracking of in foreground time for the application.
    /// </summary>
    /// <param name="inFocus"></param>
    public static void SetApplicationState(bool inFocus) => Client.SetApplicationState(inFocus);
  }
}
