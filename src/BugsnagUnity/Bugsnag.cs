using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BugsnagUnity
{
  public static class Bugsnag
  {
    static object _clientLock = new object();

    public static IClient Init(string apiKey)
    {
      lock (_clientLock)
      {
        if (Client == null)
        {
          switch (Application.platform)
          {
            case RuntimePlatform.tvOS:
            case RuntimePlatform.IPhonePlayer:
              Client = new iOSClient(new iOSConfiguration(apiKey));
              break;
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
              Client = new MacOsClient(new MacOSConfiguration(apiKey));
              break;
            case RuntimePlatform.Android:
              Client = new AndroidClient(new AndroidConfiguration(apiKey));
              break;
            default:
              Client = new Client(new Configuration(apiKey));
              break;
          }
        }
      }

      return Client;
    }

    public static IClient Client { get; private set; }
  }
}
