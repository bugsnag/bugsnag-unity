using Bugsnag.Unity.Payload;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bugsnag.Unity
{
  public interface IClient
  {
    IConfiguration Configuration { get; }

    IBreadcrumbs Breadcrumbs { get; }

    ISessionTracker SessionTracking { get; }

    User User { get; }

    void Send(IPayload payload);

    Metadata Metadata { get; }
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

    protected BaseClient(IConfiguration configuration)
    {
      Configuration = configuration;
      User = new User();
      Metadata = new Metadata();
      UniqueCounter = new UniqueLogThrottle(configuration);
      LogTypeCounter = new MaximumLogTypeCounter(configuration);
      SessionTracking = new SessionTracker(this);

      // might need to do this later?
      SceneManager.sceneLoaded += SceneLoaded;
      Application.logMessageReceivedThreaded += Notify;
      AppDomain.CurrentDomain.UnhandledException += Notify;
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
      Application.logMessageReceivedThreaded += Notify;
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
            var @event = new Payload.Event(GenerateMetadata(), GenerateAppData(), GenerateDeviceData(), User, new UnityLogExceptions(logMessage).ToArray(), HandledState.ForUnhandledException(), Breadcrumbs.Retrieve(), SessionTracking.CurrentSession);
            var report = new Report(Configuration, @event);

            Send(report);
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

    /// <summary>
    /// Notify an unhandled exception.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Notify(object sender, UnhandledExceptionEventArgs e)
    {
      // this will always be an unhandled exception
      if (Configuration.AutoNotify)
      {
        if (e.ExceptionObject is System.Exception exception)
        {
          // this will always be a handled exception?
          var @event = new Payload.Event(GenerateMetadata(), GenerateAppData(), GenerateDeviceData(), User, new Exceptions(exception).ToArray(), HandledState.ForUnhandledException(), Breadcrumbs.Retrieve(), SessionTracking.CurrentSession);
          var report = new Report(Configuration, @event);

          Send(report);
        }
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
}
