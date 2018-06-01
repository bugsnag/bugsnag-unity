using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Bugsnag.Native
{
  public static class Client
  {
    static readonly AndroidJavaClass BugsnagUnity =
      new AndroidJavaClass("com.bugsnag.android.unity.UnityClient");

    static readonly Regex unityExpression =
      new Regex("(\\S+)\\s*\\(.*?\\)\\s*(?:(?:\\[.*\\]\\s*in\\s|\\(at\\s*\\s*)(.*):(\\d+))?",
        RegexOptions.IgnoreCase | RegexOptions.Multiline);

    public static void Register(string apiKey)
    {
      Register(apiKey, false);
    }

    public static void Register(string apiKey, bool trackSessions)
    {
      var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
      var activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
      var app = activity.Call<AndroidJavaObject>("getApplicationContext");
      jvalue[] args = new jvalue[3]
      {
        new jvalue() { l = app.GetRawObject() },
        new jvalue() { l = AndroidJNI.NewStringUTF(apiKey) },
        new jvalue() { l = (IntPtr)(trackSessions ? 1 : 0) },
      };
      var methodId = AndroidJNI.GetStaticMethodID(
        BugsnagUnity.GetRawClass(),
        "init",
        "(Landroid/content/Context;Ljava/lang/String;Z)V"
      );
      AndroidJNI.CallStaticVoidMethod(BugsnagUnity.GetRawClass(), methodId, args);
    }
  }
}
