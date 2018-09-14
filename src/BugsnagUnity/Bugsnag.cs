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
        if (InternalClient == null)
        {
          switch (Application.platform)
          {
            case RuntimePlatform.tvOS:
            case RuntimePlatform.IPhonePlayer:
              //InternalClient = new Client(new CocoaClient(new iOSConfiguration(apiKey)));
              break;
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
              //InternalClient = new Client(new CocoaClient(new MacOSConfiguration(apiKey)));
              break;
            case RuntimePlatform.Android:
              //InternalClient = new Client(new AndroidClient(new AndroidConfiguration(apiKey)));
              break;
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
              //InternalClient = new Client(new WindowsClient(new Configuration(apiKey)));
              break;
            default:
              //InternalClient = new Client(new NativeClient(new Configuration(apiKey)));
              break;
          }
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

    /// <summary>
    /// Used to signal to the Bugsnag client that the focused state of the
    /// application has changed. This is used for session tracking and also
    /// the tracking of in foreground time for the application.
    /// </summary>
    /// <param name="inFocus"></param>
    public static void SetApplicationState(bool inFocus) => Client.SetApplicationState(inFocus);
  }
}
