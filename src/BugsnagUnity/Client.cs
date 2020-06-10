using BugsnagUnity.Payload;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BugsnagUnity
{
  internal class Client : IClient
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

    Stopwatch ForegroundStopwatch { get; }

    Stopwatch BackgroundStopwatch { get; }

    bool InForeground => ForegroundStopwatch.IsRunning;

    const string UnityMetadataKey = "Unity";

    private Thread MainThread;

    private static double AutoCaptureSessionThresholdSeconds = 30;

    private GameObject TimingTrackerObject { get; }

    private static object autoSessionLock = new object();

    public Client(INativeClient nativeClient)
    {
      MainThread = Thread.CurrentThread;
      ForegroundStopwatch = new Stopwatch();
      BackgroundStopwatch = new Stopwatch();
      NativeClient = nativeClient;
      User = new User { Id = SystemInfo.deviceUniqueIdentifier };
      Middleware = new List<Middleware>();
      Metadata = new Metadata(nativeClient);
      UniqueCounter = new UniqueLogThrottle(Configuration);
      LogTypeCounter = new MaximumLogTypeCounter(Configuration);
      SessionTracking = new SessionTracker(this);
      UnityMetadata.InitDefaultMetadata();
      Device.InitUnityVersion();
      NativeClient.SetMetadata(UnityMetadataKey, UnityMetadata.ForNativeClient());

      NativeClient.PopulateUser(User);

      SceneManager.sceneLoaded += SceneLoaded;
      Application.logMessageReceivedThreaded += MultiThreadedNotify;
      Application.logMessageReceived += Notify;
      User.PropertyChanged += (obj, args) => { NativeClient.SetUser(User); };
      TimingTrackerObject = new GameObject("Bugsnag app lifecycle tracker");
      TimingTrackerObject.AddComponent<TimingTrackerBehaviour>();
      // Run initial session check in next frame to allow potential configuration
      // changes to be completed first.
      try {
        var asyncHandler = MainThreadDispatchBehaviour.Instance();
        asyncHandler.Enqueue(RunInitialSessionCheck());
      } catch (System.Exception ex) {
        // Async behavior is not available in a test environment
      }
    }

    public void Send(IPayload payload)
    {
      if (!ShouldSendRequests())
      {
        return;
      }
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

    void MultiThreadedNotify(string condition, string stackTrace, LogType logType)
    {
      // Discard messages from the main thread as they will be reported separately
      if (!object.ReferenceEquals(Thread.CurrentThread, MainThread))
      {
        Notify(condition, stackTrace, logType);
      }
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
        var shouldSend = Exception.ShouldSend(logMessage)
          && UniqueCounter.ShouldSend(logMessage)
          && LogTypeCounter.ShouldSend(logMessage);
        if (shouldSend)
        {
          var severity = Configuration.LogTypeSeverityMapping.Map(logType);
          var backupStackFrames = new System.Diagnostics.StackTrace(1, true).GetFrames();
          var forceUnhandled = logType == LogType.Exception && !Configuration.ReportUncaughtExceptionsAsHandled;
          var exception = Exception.FromUnityLogMessage(logMessage, backupStackFrames, severity, forceUnhandled);
          Notify(new Exception[]{exception}, exception.HandledState, null, logType);
        }
      }
      else if (logType.IsGreaterThanOrEqualTo(Configuration.BreadcrumbLogLevel))
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
      if (!ShouldSendRequests())
      {
        return; // Skip overhead of computing payload to to ultimately not be sent
      }
      var user = new User { Id = User.Id, Email = User.Email, Name = User.Name };
      var app = new App(Configuration)
      {
        InForeground = InForeground,
        DurationInForeground = ForegroundStopwatch.Elapsed,
      };
      NativeClient.PopulateApp(app);
      var device = new Device();
      NativeClient.PopulateDevice(device);
      device.AddRuntimeVersions(Configuration);

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
        ForegroundStopwatch.Start();
        lock (autoSessionLock)
        {
          if (Configuration.AutoCaptureSessions
           && BackgroundStopwatch.Elapsed.TotalSeconds > AutoCaptureSessionThresholdSeconds)
          {
            SessionTracking.StartSession();
          }
          BackgroundStopwatch.Reset();
        }
      }
      else
      {
        ForegroundStopwatch.Stop();
        BackgroundStopwatch.Start();
      }
    }

    /// <summary>
    /// True if reports and sessions should be sent based on release stage settings
    /// </summary>
    private bool ShouldSendRequests()
    {
      return Configuration.ReleaseStage == null
          || Configuration.NotifyReleaseStages == null
          || Configuration.NotifyReleaseStages.Contains(Configuration.ReleaseStage);
    }

    /// <summary>
    /// Check next frame if a new session should be captured
    /// </summary>
    private IEnumerator<UnityEngine.AsyncOperation> RunInitialSessionCheck()
    {
      yield return null;
      if (Configuration.AutoCaptureSessions && SessionTracking.CurrentSession == null)
      {
        SessionTracking.StartSession();
      }
    }
  }
}
