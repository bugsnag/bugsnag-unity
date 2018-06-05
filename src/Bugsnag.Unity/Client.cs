using System;
using UnityEngine;
using Bugsnag.Unity.Payload;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Bugsnag.Unity
{
  public class Client
  {
    /// <summary>
    /// Used to control access to configuring the client singleton
    /// </summary>
    private static object Lock { get; } = new object();

    /// <summary>
    /// Has the client singleton been configured
    /// </summary>
    private static bool Configured
    {
      get
      {
        return Instance.Configuration != null;
      }
    }

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static Client()
    {
    }

    private Client()
    {
    }

    public static Client Instance { get; } = new Client();

    public Configuration Configuration { get; private set; }

    public Breadcrumbs Breadcrumbs { get; private set; }

    public SessionTracker SessionTracking { get; private set; }

    private UniqueLogThrottle UniqueCounter { get; set; }

    private MaximumLogTypeCounter LogTypeCounter { get; set; }

    public User User { get; private set; }

    /// <summary>
    /// Initializes the singleton client with the provided configuration.
    /// Must be called before calling other methods on the client.
    /// </summary>
    /// <param name="configuration"></param>
    public void Init(Configuration configuration)
    {
      lock (Lock)
      {
        if (!Configured)
        {
          SessionTracking = new SessionTracker(this);
          Breadcrumbs = new Breadcrumbs(configuration);
          UniqueCounter = new UniqueLogThrottle(configuration);
          LogTypeCounter = new MaximumLogTypeCounter(configuration);
          User = new User();
          Native.Client.Register(configuration.ApiKey);

          // set the configuration after the other properties so that we can
          // use the Configured check to skip methods until the client is
          // configured
          Configuration = configuration;

          // now we can hook up these events as the methods depend on all of
          // above being setup first.
          Application.logMessageReceivedThreaded += Notify;
          // handle multiple versions here or just say we only support X
          //Application.RegisterLogCallbackThreaded(Notify);
          AppDomain.CurrentDomain.UnhandledException += Notify;
          SceneManager.sceneLoaded += SceneLoaded;
        }
      }
    }

    /// <summary>
    /// Sets the current context to the scene name and leaves a breadcrumb with
    /// the current scene information.
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="loadSceneMode"></param>
    private void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
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
    private void Notify(string condition, string stackTrace, LogType logType)
    {
      if (logType.IsGreaterThanOrEqualTo(Configuration.NotifyLevel))
      {
        var logMessage = new UnityLogMessage(condition, stackTrace, logType);

        if (UniqueCounter.ShouldSend(logMessage))
        {
          if (LogTypeCounter.ShouldSend(logMessage))
          {
            var report = new Report(Configuration, logMessage, HandledState.ForUnhandledException(), Breadcrumbs.Retrieve(), SessionTracking.CurrentSession);

            Notify(report);
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
    private void Notify(object sender, UnhandledExceptionEventArgs e)
    {
      // this will always be an unhandled exception
      if (Configuration.AutoNotify)
      {
        if (e.ExceptionObject is System.Exception exception)
        {
          // this will always be a handled exception?
          var report = new Report(Configuration, exception, HandledState.ForUnhandledException(), Breadcrumbs.Retrieve(), SessionTracking.CurrentSession);

          Notify(report);
        }
      }
    }

    /// <summary>
    /// Notify a handled exception. Init must be called on the client first.
    /// </summary>
    /// <param name="exception"></param>
    public void Notify(System.Exception exception)
    {
      if (Configured)
      {
        // this will always be a handled exception?
        var report = new Report(Configuration, exception, HandledState.ForHandledException(), Breadcrumbs.Retrieve(), SessionTracking.CurrentSession);

        Notify(report);
      }
    }

    /// <summary>
    /// Notify a complete Bugsnag report. Init must be called on the client first.
    /// </summary>
    /// <param name="report"></param>
    public void Notify(Report report)
    {
      if (Configured)
      {
        report.Event.Metadata.Add("Unity", UnityMetadata.Data);
        // callback time
        ThreadQueueDelivery.Instance.Send(report);
        // add a breadcrumb about the report
      }
    }
  }
}
