using BugsnagUnity.Payload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

  abstract class BaseClient : IClient
  {
    public IConfiguration Configuration { get; }

    public abstract IBreadcrumbs Breadcrumbs { get; }

    public ISessionTracker SessionTracking { get; }

    public User User { get; }

    public Metadata Metadata { get; }

    UniqueLogThrottle UniqueCounter { get; }

    MaximumLogTypeCounter LogTypeCounter { get; }

    protected abstract IDelivery Delivery { get; }

    protected abstract App GenerateAppData();

    protected abstract Device GenerateDeviceData();

    protected abstract Metadata GenerateMetadata();

    List<Middleware> Middleware { get; }

    object MiddlewareLock { get; } = new object();

    protected BaseClient(IConfiguration configuration)
    {
      Configuration = configuration;
      User = new User();
      Middleware = new List<Middleware>();
      Metadata = new Metadata();
      UniqueCounter = new UniqueLogThrottle(configuration);
      LogTypeCounter = new MaximumLogTypeCounter(configuration);
      SessionTracking = new SessionTracker(this);

      // might need to do this later?
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
            var user = new User { Id = User.Id, Email = User.Email, Name = User.Name };
            var @event = new Payload.Event(Configuration.Context,
              GenerateMetadata(),
              GenerateAppData(),
              GenerateDeviceData(),
              user,
              new UnityLogExceptions(logMessage).ToArray(),
              HandledState.ForHandledException(),
              Breadcrumbs.Retrieve(), SessionTracking.CurrentSession,
              logMessage.Type);
            var report = new Report(Configuration, @event);

            Notify(report, null);
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
      var user = new User { Id = User.Id, Email = User.Email, Name = User.Name };
      var @event = new Payload.Event(Configuration.Context,
        GenerateMetadata(),
        GenerateAppData(),
        GenerateDeviceData(),
        user,
        new Exceptions(exception).ToArray(),
        handledState,
        Breadcrumbs.Retrieve(),
        SessionTracking.CurrentSession);
      var report = new Report(Configuration, @event);
      Notify(report, callback);
    }

    void Notify(Report report, Middleware callback)
    {
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

  class Client : BaseClient
  {
    internal Client(IConfiguration configuration) : base(configuration)
    {
      Delivery = new Delivery();
      Breadcrumbs = new Breadcrumbs(configuration);

      var unityMetadata = UnityMetadata.Data;
      Metadata.Add("Unity", unityMetadata);
    }

    public override IBreadcrumbs Breadcrumbs { get; }

    protected override IDelivery Delivery { get; }

    protected override App GenerateAppData()
    {
      return new App(Configuration);
    }

    protected override Device GenerateDeviceData()
    {
      return new Device();
    }

    protected override Metadata GenerateMetadata()
    {
      return new Metadata(Metadata);
    }
  }

  class AndroidClient : BaseClient
  {
    internal AndroidJavaObject JavaClient { get; }

    internal AndroidClient(AndroidConfiguration configuration) : base(configuration)
    {
      using (var notifier = new AndroidJavaClass("com.bugsnag.android.Notifier"))
      using (var info = notifier.CallStatic<AndroidJavaObject>("getInstance"))
      {
        info.Call("setURL", NotifierInfo.NotifierUrl);
        info.Call("setName", NotifierInfo.NotifierName);
        info.Call("setVersion", NotifierInfo.NotifierVersion);
      }

      using (var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
      using (var activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"))
      using (var context = activity.Call<AndroidJavaObject>("getApplicationContext"))
      {
        JavaClient = new AndroidJavaObject("com.bugsnag.android.Client", context, configuration.JavaObject);

        // the bugsnag-android notifier uses Activity lifecycle tracking to
        // determine if the application is in the foreground. As the unity
        // activity has already started at this point we need to tell the
        // notifier about the activity and the fact that it has started.
        using (var sessionTracker = JavaClient.Get<AndroidJavaObject>("sessionTracker"))
        using (var activityClass = activity.Call<AndroidJavaObject>("getClass"))
        {
          var activityName = activityClass.Call<string>("getSimpleName");
          sessionTracker.Call("updateForegroundTracker", activityName, true, 0L);
        }
      }

      Delivery = new AndroidDelivery();
      Breadcrumbs = new AndroidBreadcrumbs(JavaClient);

      using (var user = JavaClient.Call<AndroidJavaObject>("getUser"))
      {
        User.Id = user.Call<string>("getId");
        User.Name = user.Call<string>("getName");
        User.Email = user.Call<string>("getEmail");
      }

      var unityMetadata = UnityMetadata.Data;
      Metadata.Add("Unity", unityMetadata);

      using (var metadata = JavaClient.Call<AndroidJavaObject>("getMetaData"))
      {
        foreach (var item in unityMetadata)
        {
          metadata.Call("addToTab", "Unity", item.Key, item.Value);
        }
      }
    }

    public override IBreadcrumbs Breadcrumbs { get; }

    protected override IDelivery Delivery { get; }

    protected override App GenerateAppData()
    {
      return new AndroidApp(Configuration, JavaClient);
    }

    protected override Device GenerateDeviceData()
    {
      return new AndroidDevice(JavaClient);
    }

    protected override Metadata GenerateMetadata()
    {
      return new AndroidMetadata(JavaClient, Metadata);
    }
  }

  class MacOsClient : BaseClient
  {
    [DllImport("bugsnag-osx", EntryPoint = "bugsnag_startBugsnagWithConfiguration")]
    static extern void StartBugsnagWithConfiguration(IntPtr configuration);

    [DllImport("bugsnag-osx", EntryPoint = "bugsnag_setMetadata")]
    static extern void AddMetadata(IntPtr configuration, string tab, string[] metadata, int metadataCount);

    internal MacOsClient(MacOSConfiguration configuration) : base(configuration)
    {
      var unityMetadata = UnityMetadata.Data;
      Metadata.Add("Unity", unityMetadata);

      var index = 0;
      var metadata = new string[unityMetadata.Count * 2];

      foreach (var data in unityMetadata)
      {
        metadata[index] = data.Key;
        metadata[index + 1] = data.Value;
        index += 2;
      }

      AddMetadata(configuration.NativeConfiguration, "Unity", metadata, metadata.Length);

      StartBugsnagWithConfiguration(configuration.NativeConfiguration);

      Delivery = new Delivery();
      Breadcrumbs = new MacOsBreadcrumbs(configuration);
    }

    public override IBreadcrumbs Breadcrumbs { get; }

    protected override IDelivery Delivery { get; }

    protected override App GenerateAppData()
    {
      return new MacOsApp(Configuration);
    }

    protected override Device GenerateDeviceData()
    {
      return new MacOsDevice();
    }

    protected override Metadata GenerateMetadata()
    {
      return new Metadata(Metadata);
    }
  }

  class iOSClient : BaseClient
  {
    [DllImport("__Internal", EntryPoint = "bugsnag_startBugsnagWithConfiguration")]
    static extern void StartBugsnagWithConfiguration(IntPtr configuration);

    [DllImport("__Internal", EntryPoint = "bugsnag_setMetadata")]
    static extern void AddMetadata(IntPtr configuration, string tab, string[] metadata, int metadataCount);

    public iOSClient(iOSConfiguration configuration) : base(configuration)
    {
      var unityMetadata = UnityMetadata.Data;
      Metadata.Add("Unity", unityMetadata);

      var index = 0;
      var metadata = new string[unityMetadata.Count * 2];

      foreach (var data in unityMetadata)
      {
        metadata[index] = data.Key;
        metadata[index + 1] = data.Value;
        index += 2;
      }

      AddMetadata(configuration.NativeConfiguration, "Unity", metadata, metadata.Length);

      StartBugsnagWithConfiguration(configuration.NativeConfiguration);

      Delivery = new Delivery();
      Breadcrumbs = new iOSBreadcrumbs(configuration);
    }

    public override IBreadcrumbs Breadcrumbs { get; }

    protected override IDelivery Delivery { get; }

    protected override App GenerateAppData()
    {
      return new iOSApp(Configuration);
    }

    protected override Device GenerateDeviceData()
    {
      return new iOSDevice();
    }

    protected override Metadata GenerateMetadata()
    {
      return new Metadata(Metadata);
    }
  }
}
