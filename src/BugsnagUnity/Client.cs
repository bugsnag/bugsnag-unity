using BugsnagUnity.Payload;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BugsnagUnity
{
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

    internal INativeClient NativeClient { get; }

    Stopwatch Stopwatch { get; }

    bool InForeground => Stopwatch.IsRunning;

    const string UnityMetadataKey = "Unity";

    public Client(INativeClient nativeClient)
    {
      Stopwatch = new Stopwatch();
      NativeClient = nativeClient;
      User = new User { Id = SystemInfo.deviceUniqueIdentifier };
      Middleware = new List<Middleware>();
      Metadata = new Metadata();
      UniqueCounter = new UniqueLogThrottle(Configuration);
      LogTypeCounter = new MaximumLogTypeCounter(Configuration);
      SessionTracking = new SessionTracker(this);
      NativeClient.SetMetadata(UnityMetadataKey, UnityMetadata.ForNativeClient());

      NativeClient.PopulateUser(User);

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
      if (Configuration.AutoNotify && logType.IsGreaterThanOrEqualTo(Configuration.NotifyLevel))
      {
        var logMessage = new UnityLogMessage(condition, stackTrace, logType);

        if (UniqueCounter.ShouldSend(logMessage))
        {
          if (LogTypeCounter.ShouldSend(logMessage))
          {
            var severity = Configuration.LogTypeSeverityMapping.Map(logType);
            var backupStackFrames = new System.Diagnostics.StackTrace(1, true).GetFrames();
            var exception = Exception.FromUnityLogMessage(logMessage, backupStackFrames, severity);
            Notify(new Exception[]{exception}, exception.HandledState, null, logType);
          }
        }
      }
      else
      {
        Breadcrumbs.Leave(logType.ToString(), BreadcrumbType.Log, new Dictionary<string, string> {
          { "message", condition },
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
      Notify(exception, 3);
    }

    internal void Notify(System.Exception exception, int level)
    {
      Notify(exception, HandledState.ForHandledException(), null, level);
    }

    public void Notify(System.Exception exception, Middleware callback)
    {
      Notify(exception, callback, 3);
    }

    internal void Notify(System.Exception exception, Middleware callback, int level)
    {
      Notify(exception, HandledState.ForHandledException(), callback, level);
    }

    public void Notify(System.Exception exception, Severity severity)
    {
      Notify(exception, severity, 3);
    }

    internal void Notify(System.Exception exception, Severity severity, int level)
    {
      Notify(exception, HandledState.ForUserSpecifiedSeverity(severity), null, level);
    }

    public void Notify(System.Exception exception, Severity severity, Middleware callback)
    {
      Notify(exception, severity, callback, 3);
    }

    internal void Notify(System.Exception exception, Severity severity, Middleware callback, int level)
    {
      Notify(exception, HandledState.ForUserSpecifiedSeverity(severity), callback, level);
    }

    void Notify(System.Exception exception, HandledState handledState, Middleware callback, int level)
    {
      // we need to generate a substitute stacktrace here as if we are not able
      // to generate one from the exception that we are given then we are not able
      // to do this inside of the IEnumerator generator code
      var substitute = new System.Diagnostics.StackTrace(level, true).GetFrames();
      Notify(new Exceptions(exception, substitute).ToArray(), handledState, callback, null);
    }

    void Notify(Exception[] exceptions, HandledState handledState, Middleware callback, LogType? logType)
    {
      var user = new User { Id = User.Id, Email = User.Email, Name = User.Name };
      var app = new App(Configuration)
      {
        InForeground = InForeground,
        DurationInForeground = Stopwatch.Elapsed,
      };
      NativeClient.PopulateApp(app);
      var device = new Device();
      NativeClient.PopulateDevice(device);
      var metadata = new Metadata();
      NativeClient.PopulateMetadata(metadata);

      foreach (var item in Metadata)
      {
        metadata.AddToPayload(item.Key, item.Value);
      }

      metadata.AddToPayload(UnityMetadataKey, UnityMetadata.WithLogType(logType));

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
        return;
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

        SessionTracking.AddException(report);
      }
    }

    public void SetApplicationState(bool inFocus)
    {
      if (inFocus)
      {
        Stopwatch.Start();

        if (Configuration.AutoCaptureSessions)
        {
          SessionTracking.StartSession();
        }
      }
      else
      {
        Stopwatch.Stop();
      }
    }
  }
}
