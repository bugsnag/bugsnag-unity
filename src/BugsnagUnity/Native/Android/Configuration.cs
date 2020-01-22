using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity
{
  class Configuration : AbstractConfiguration
  {
    // Cached value of native-layer auto notify configuration setting to reduce the
    // number of native calls required when reporting a Unity error
    private bool _autoNotify;
    private bool _autoDetectAnrs = false;

    internal NativeInterface NativeInterface { get; }

    internal Configuration(string apiKey, bool autoNotify) : base()
    {
      var JavaObject = new AndroidJavaObject("com.bugsnag.android.Configuration", apiKey);
      // the bugsnag-unity notifier will handle session tracking
      JavaObject.Call("setAutoCaptureSessions", false);
      JavaObject.Call("setEnableExceptionHandler", _autoNotify);
      JavaObject.Call("setDetectAnrs", _autoDetectAnrs);
      JavaObject.Call("setCallPreviousSigquitHandler", false);
      JavaObject.Call("setDetectNdkCrashes", _autoNotify);
      JavaObject.Call("setEndpoint", DefaultEndpoint);
      JavaObject.Call("setSessionEndpoint", DefaultSessionEndpoint);
      JavaObject.Call("setReleaseStage", "production");
      JavaObject.Call("setAppVersion", Application.version);
      NativeInterface = new NativeInterface(JavaObject);
      SetupDefaults(apiKey);
      _autoNotify = autoNotify;
    }

    protected override void SetupDefaults(string apiKey)
    {
      base.SetupDefaults(apiKey);
      ReleaseStage = "production";
      Endpoint = new Uri(DefaultEndpoint);
      SessionEndpoint = new Uri(DefaultSessionEndpoint);
    }

    public override bool AutoNotify
    {
      get => _autoNotify;
      set {
        _autoNotify = value;
        NativeInterface.SetAutoNotify(_autoNotify);
        NativeInterface.SetAutoDetectAnrs(_autoDetectAnrs && _autoNotify);
      }
    }

    public override bool AutoDetectAnrs
    {      
      get => _autoDetectAnrs;
      set {
        _autoDetectAnrs = value;
        NativeInterface.SetAutoDetectAnrs(_autoDetectAnrs && _autoNotify);
      }
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
