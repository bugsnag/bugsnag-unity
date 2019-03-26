using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity
{
  public class Configuration : AbstractConfiguration
  {
    internal NativeInterface NativeInterface { get; }

    public Configuration(string apiKey) : base()
    {
      var JavaObject = new AndroidJavaObject("com.bugsnag.android.Configuration", apiKey);
      // the bugsnag-unity notifier will handle session tracking
      JavaObject.Call("setAutoCaptureSessions", false);
      JavaObject.Call("setEndpoint", DefaultEndpoint);
      JavaObject.Call("setSessionEndpoint", DefaultSessionEndpoint);
      JavaObject.Call("setReleaseStage", "production");
      JavaObject.Call("setAppVersion", Application.version);
      NativeInterface = new NativeInterface(JavaObject);
      SetupDefaults(apiKey);
    }

    protected override void SetupDefaults(string apiKey)
    {
      base.SetupDefaults(apiKey);
      ReleaseStage = "production";
      Endpoint = new Uri(DefaultEndpoint);
      SessionEndpoint = new Uri(DefaultSessionEndpoint);
    }

    public override string ReleaseStage
    {
      set
      {
        NativeInterface.SetReleaseStage(value);
      }
      get
      {
        return NativeInterface.GetReleaseStage();
      }
    }

    public override string[] NotifyReleaseStages
    {
      set
      {
        NativeInterface.SetNotifyReleaseStages(value);
      }
      get
      {
        return NativeInterface.GetNotifyReleaseStages();
      }
    }

    public override string AppVersion
    {
      set
      {
        NativeInterface.SetAppVersion(value);
      }
      get
      {
        return NativeInterface.GetAppVersion();
      }
    }

    public override bool DetectAnrs {
      set
      {
        NativeInterface.SetDetectAnrs(value);
      }
      get
      {
        return NativeInterface.GetDetectAnrs();
      }
    }

    public override long AnrThresholdMs {
      set
      {
        NativeInterface.SetAnrThresholdMs(value);
      }
      get
      {
        return NativeInterface.GetAnrThresholdMs();
      }
    }

    public override Uri Endpoint
    {
      set
      {
        NativeInterface.SetEndpoint(value.ToString());
      }
      get
      {
        return new Uri(NativeInterface.GetEndpoint());
      }
    }

    public override Uri SessionEndpoint
    {
      set
      {
        NativeInterface.SetSessionEndpoint(value.ToString());
      }
      get
      {
        return new Uri(NativeInterface.GetSessionEndpoint());
      }
    }

    public override string Context
    {
      set
      {
        NativeInterface.SetContext(value);
      }
      get
      {
        return NativeInterface.GetContext();
      }
    }
  }
}
