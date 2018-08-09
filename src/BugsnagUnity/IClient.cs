using BugsnagUnity.Payload;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BugsnagUnity
{
  public delegate void Middleware(Report report);

  public interface IClient
  {
    IConfiguration Configuration { get; }

    IBreadcrumbs Breadcrumbs { get; }

    ISessionTracker SessionTracking { get; }

    User User { get; }

    void Send(IPayload payload);

    Metadata Metadata { get; }

    void BeforeNotify(Middleware middleware);

    void Notify(System.Exception exception);

    void Notify(System.Exception exception, Middleware callback);

    void Notify(System.Exception exception, Severity severity);

    void Notify(System.Exception exception, Severity severity, Middleware callback);
  }

  interface INativeClient
  {
    /// <summary>
    /// The native configuration
    /// </summary>
    IConfiguration Configuration { get; }

    /// <summary>
    /// The native breadcrumbs
    /// </summary>
    IBreadcrumbs Breadcrumbs { get; }

    /// <summary>
    /// The native delivery method
    /// </summary>
    IDelivery Delivery { get; }

    /// <summary>
    /// Populates the native app information
    /// </summary>
    /// <returns></returns>
    void PopulateApp(App app);

    /// <summary>
    /// Populates the native device information
    /// </summary>
    /// <returns></returns>
    void PopulateDevice(Device device);

    /// <summary>
    /// Adds the metadata to the native clients metadata
    /// </summary>
    /// <param name="metadata"></param>
    void SetMetadata(string tab, Dictionary<string, string> metadata);

    /// <summary>
    /// Populates the native user information.
    /// </summary>
    /// <returns></returns>
    void PopulateUser(User user);

    /// <summary>
    /// Populates any native metadata.
    /// </summary>
    /// <param name="metadata"></param>
    void PopulateMetadata(Metadata metadata);
  }

  class NativeClient : INativeClient
  {
    public IConfiguration Configuration { get; }

    public IBreadcrumbs Breadcrumbs { get; }

    public IDelivery Delivery { get; }

    public NativeClient(Configuration configuration)
    {
      Configuration = configuration;
      Breadcrumbs = new Breadcrumbs(configuration);
      Delivery = new Delivery();
    }

    public void PopulateApp(App app)
    {
    }

    public void PopulateDevice(Device device)
    {
    }

    public void PopulateMetadata(Metadata metadata)
    {
    }

    public void PopulateUser(User user)
    {
    }

    public void SetMetadata(string tab, Dictionary<string, string> metadata)
    {
    }
  }

  class Client : IClient
  {
    public IConfiguration Configuration => NativeClient.Configuration;

    public IBreadcrumbs Breadcrumbs => NativeClient.Breadcrumbs;

    public ISessionTracker SessionTracking { get; }

    public User User { get; }

    public Metadata Metadata { get; }

    UniqueLogThrottle UniqueCounter { get; }

    MaximumLogTypeCounter LogTypeCounter { get; }

    protected IDelivery Delivery => NativeClient.Delivery;

    List<Middleware> Middleware { get; }

    object MiddlewareLock { get; } = new object();

    INativeClient NativeClient { get; }

    public Client(INativeClient nativeClient)
    {
      NativeClient = nativeClient;
      User = new User();
      Middleware = new List<Middleware>();
      Metadata = new Metadata();
      UniqueCounter = new UniqueLogThrottle(Configuration);
      LogTypeCounter = new MaximumLogTypeCounter(Configuration);
      SessionTracking = new SessionTracker(this);
      var unityMetadata = UnityMetadata.Data;
      Metadata.Add("Unity", unityMetadata);
      NativeClient.SetMetadata("Unity", unityMetadata);
      SceneManager.sceneLoaded += SceneLoaded;
      Application.logMessageReceivedThreaded += Notify;
    }

    public void Send(IPayload payload)
    {
      Delivery.Send(payload);
    }

    /// <summary>
    /// Sets the current context to the scene name and leaves a breadcrumb with
    /// the current scene information.
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="loadSceneMode"></param>
    void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
      Configuration.Context = scene.name;
      Breadcrumbs.Leave("Scene Loaded", BreadcrumbType.State, new Dictionary<string, string> { { "sceneName", scene.name } });
    }

    /// <summary>
    /// Notify a Unity log message if it the client has been configured to
    /// notify at the specified level, if not leave a breadcrumb with the log
    /// message.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="stackTrace"></param>
    /// <param name="logType"></param>
    void Notify(string condition, string stackTrace, LogType logType)
    {
      if (logType.IsGreaterThanOrEqualTo(Configuration.NotifyLevel))
      {
        var logMessage = new UnityLogMessage(condition, stackTrace, logType);

        if (UniqueCounter.ShouldSend(logMessage))
        {
          if (LogTypeCounter.ShouldSend(logMessage))
          {
            Notify(new UnityLogExceptions(logMessage).ToArray(), HandledState.ForHandledException(), null, logMessage.Type);
          }
        }
      }
      else
      {
        Breadcrumbs.Leave(logType.ToString(), BreadcrumbType.Log, new Dictionary<string, string> {
          { "condition", condition },
          { "stackTrace", stackTrace },
        });
      }
    }

    public void BeforeNotify(Middleware middleware)
    {
      lock (MiddlewareLock)
      {
        Middleware.Add(middleware);
      }
    }

    public void Notify(System.Exception exception)
    {
      Notify(exception, null);
    }

    public void Notify(System.Exception exception, Middleware callback)
    {
      Notify(exception, HandledState.ForHandledException(), callback);
    }

    public void Notify(System.Exception exception, Severity severity)
    {
      Notify(exception, severity, null);
    }

    public void Notify(System.Exception exception, Severity severity, Middleware callback)
    {
      Notify(exception, HandledState.ForUserSpecifiedSeverity(severity), callback);
    }

    void Notify(System.Exception exception, HandledState handledState, Middleware callback)
    {
      Notify(new Exceptions(exception).ToArray(), handledState, callback);
    }

    void Notify(Exception[] exceptions, HandledState handledState, Middleware callback, LogType? logType = null)
    {
      var user = new User { Id = User.Id, Email = User.Email, Name = User.Name };
      var app = new App(Configuration);
      NativeClient.PopulateApp(app);
      var device = new Device();
      NativeClient.PopulateDevice(device);
      var metadata = new Metadata();
      NativeClient.PopulateMetadata(metadata);

      foreach (var item in Metadata)
      {
        metadata.Add(item.Key, item.Value);
      }

      var @event = new Payload.Event(
        Configuration.Context,
        metadata,
        app,
        device,
        user,
        exceptions,
        handledState,
        Breadcrumbs.Retrieve(),
        SessionTracking.CurrentSession);
      var report = new Report(Configuration, @event);

      if (report.Configuration.ReleaseStage != null
          && report.Configuration.NotifyReleaseStages != null
          && !report.Configuration.NotifyReleaseStages.Contains(report.Configuration.ReleaseStage))
      {
        report.Ignore();
      }

      lock (MiddlewareLock)
      {
        foreach (var middleware in Middleware)
        {
          try
          {
            middleware(report);
          }
          catch (System.Exception)
          {
          }
        }
      }

      try
      {
        callback?.Invoke(report);
      }
      catch (System.Exception)
      {
      }

      if (!report.Ignored)
      {
        Send(report);

        Breadcrumbs.Leave(Breadcrumb.FromReport(report));

        SessionTracking.CurrentSession?.AddException(report);
      }
    }
  }
}
