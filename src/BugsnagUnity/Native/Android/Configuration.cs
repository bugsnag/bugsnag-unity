using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity
{
  class Configuration : AbstractConfiguration
  {
    internal NativeInterface NativeInterface { get; }

    internal Configuration(string apiKey) : base(apiKey)
    {
      var JavaObject = new AndroidJavaObject("com.bugsnag.android.Configuration", apiKey);
      // the bugsnag-unity notifier will handle session tracking
      JavaObject.Call("setAutoCaptureSessions", false);
      JavaObject.Call("setEndpoint", DefaultEndpoint);
      JavaObject.Call("setSessionEndpoint", DefaultSessionEndpoint);
      JavaObject.Call("setReleaseStage", "production");
      JavaObject.Call("setAppVersion", Application.version);
      NativeInterface = new NativeInterface(JavaObject);
    }

    public new string ReleaseStage
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

    public new string[] NotifyReleaseStages
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

    public new string AppVersion
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

    public new Uri Endpoint
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

    public new Uri SessionEndpoint
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

    public new string Context
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
