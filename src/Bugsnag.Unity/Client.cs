using System;
using UnityEngine;
using Bugsnag.Unity.Payload;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.SceneManagement;

namespace Bugsnag.Unity
{
  public class Client
  {
    static Client()
    {
    }

    private Client()
    {
      Configuration = new Configuration();
      SessionTracking = new SessionTracker(this);
      Breadcrumbs = new Breadcrumbs(Configuration);
      UniqueCounter = new UniqueLogThrottle(Configuration);
      LogTypeCounter = new MaximumLogTypeCounter(Configuration);
      User = new User();
      // handle multiple versions here or just say we only support X
      //Application.RegisterLogCallbackThreaded(Notify);
      Application.logMessageReceivedThreaded += Notify;
      AppDomain.CurrentDomain.UnhandledException += Notify;
      SceneManager.sceneLoaded += SceneLoaded;
    }

    private void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
      Configuration.Context = scene.name;
      Breadcrumbs.Leave("Scene Loaded", BreadcrumbType.State, new Dictionary<string, string> { { "sceneName", scene.name } });
    }

    public static Client Instance { get; } = new Client();

    public Configuration Configuration { get; }

    public Breadcrumbs Breadcrumbs { get; }

    public SessionTracker SessionTracking { get; }

    private UniqueLogThrottle UniqueCounter { get; }

    private MaximumLogTypeCounter LogTypeCounter { get; }

    public User User { get; }

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

    private void Notify(object sender, UnhandledExceptionEventArgs e)
    {
      // this will always be an unhandled exception
      if (Configuration.AutoNotify)
      {
        if (e.ExceptionObject is System.Exception exception)
        {
          Notify(exception);
        }
      }
    }

    public void Notify(System.Exception exception)
    {
      // this will always be a handled exception?
      var report = new Report(Configuration, exception, HandledState.ForUnhandledException(), Breadcrumbs.Retrieve(), SessionTracking.CurrentSession);

      Notify(report);
    }

    public void Notify(Report report)
    {
      report.Event.Metadata.Add("Unity", UnityMetadata.Data);
      // callback time
      ThreadQueueDelivery.Instance.Send(report);
      // add a breadcrumb about the report
    }
  }

  public class UnityMetadata
  {
    public static Dictionary<string, string> Data => new Dictionary<string, string> {
      { "unityException", "false" },
      { "unityVersion", Application.unityVersion },
      { "platform", Application.platform.ToString() },
      { "osLanguage", Application.systemLanguage.ToString() },
      { "bundleIdentifier", Application.identifier },
      //{ "bundleIdentifier", Application.bundleIdentifier },// this would seem to be the property in older versions of unity
      { "version", Application.version },
      { "companyName", Application.companyName },
      { "productName", Application.productName },
      { "threadId", Thread.CurrentThread.ManagedThreadId.ToString() },
      { "threadName", Thread.CurrentThread.Name },
    };
  }
}
