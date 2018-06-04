using UnityEngine;
using System.Collections.Generic;
using System.Threading;

namespace Bugsnag.Unity
{
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
