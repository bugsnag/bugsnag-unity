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

  class Client : IClient
  {
    internal Client(IConfiguration configuration)
    {
      Configuration = configuration;
      Delivery = new Delivery();
      User = new User();
      Metadata = new Metadata();
      SessionTracking = new SessionTracker(this);
      Breadcrumbs = new Breadcrumbs(configuration);
      UniqueCounter = new UniqueLogThrottle(configuration);
      LogTypeCounter = new MaximumLogTypeCounter(configuration);

      var unityMetadata = UnityMetadata.Data;

      Metadata.Add("Unity", unityMetadata);

      SceneManager.sceneLoaded += SceneLoaded;
      Application.logMessageReceivedThreaded += Notify;
      AppDomain.CurrentDomain.UnhandledException += Notify;
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
            var @event = new Payload.Event(Metadata, new App(Configuration), new Device(), User, new UnityLogExceptions(logMessage).ToArray(), HandledState.ForUnhandledException(), Breadcrumbs.Retrieve(), SessionTracking.CurrentSession);
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
          var @event = new Payload.Event(Metadata, new App(Configuration), new Device(), User, new Exceptions(exception).ToArray(), HandledState.ForUnhandledException(), Breadcrumbs.Retrieve(), SessionTracking.CurrentSession);
          var report = new Report(Configuration, @event);

          Send(report);
        }
      }
    }

    public void Send(IPayload payload)
    {
      Delivery.Send(payload);
    }

    public IConfiguration Configuration { get; }

    public IBreadcrumbs Breadcrumbs { get; }

    public ISessionTracker SessionTracking { get; }

    UniqueLogThrottle UniqueCounter { get; }

    MaximumLogTypeCounter LogTypeCounter { get; }

    IDelivery Delivery { get; }

    public User User { get; }

    public Metadata Metadata { get; }
  }

  class AndroidClient : IClient
  {
    internal AndroidJavaObject JavaObject { get; }

    internal AndroidClient(AndroidConfiguration configuration)
    {
      using (var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
      using (var activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"))
      using (var context = activity.Call<AndroidJavaObject>("getApplicationContext"))
      {
        JavaObject = new AndroidJavaObject("com.bugsnag.android.Client", context, configuration.JavaObject);
      }

      Configuration = configuration;
      Delivery = new AndroidDelivery();
      User = new User();
      Metadata = new Metadata();
      SessionTracking = new SessionTracker(this);
      Breadcrumbs = new AndroidBreadcrumbs(this);
      UniqueCounter = new UniqueLogThrottle(configuration);
      LogTypeCounter = new MaximumLogTypeCounter(configuration);

      using (var user = JavaObject.Call<AndroidJavaObject>("getUser")) 
      {
        User.Id = user.Call<string>("getId");
        User.Name = user.Call<string>("getName");
        User.Email = user.Call<string>("getEmail");
      }

      var unityMetadata = UnityMetadata.Data;

      Metadata.Add("Unity", unityMetadata);

      using (var metadata = JavaObject.Call<AndroidJavaObject>("getMetaData"))
      {
        foreach (var item in unityMetadata)
        {
          metadata.Call("addToTab", "Unity", item.Key, item.Value);
        }
      }

      SceneManager.sceneLoaded += SceneLoaded;
      Application.logMessageReceivedThreaded += Notify;
      AppDomain.CurrentDomain.UnhandledException += Notify;
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
            var @event = new Payload.Event(Metadata, new AndroidApp(Configuration, JavaObject), new AndroidDevice(JavaObject), User, new UnityLogExceptions(logMessage).ToArray(), HandledState.ForUnhandledException(), Breadcrumbs.Retrieve(), SessionTracking.CurrentSession);
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
          var @event = new Payload.Event(Metadata, new AndroidApp(Configuration, JavaObject), new AndroidDevice(JavaObject), User, new Exceptions(exception).ToArray(), HandledState.ForUnhandledException(), Breadcrumbs.Retrieve(), SessionTracking.CurrentSession);
          var report = new Report(Configuration, @event);

          Send(report);
        }
      }
    }

    public void Send(IPayload payload)
    {
      Delivery.Send(payload);
    }

    public IConfiguration Configuration { get; }

    public IBreadcrumbs Breadcrumbs { get; }

    public ISessionTracker SessionTracking { get; }

    UniqueLogThrottle UniqueCounter { get; }

    MaximumLogTypeCounter LogTypeCounter { get; }

    IDelivery Delivery { get; }

    public User User { get; }

    public Metadata Metadata { get; }
  }
}
