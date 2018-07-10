using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bugsnag.Unity
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
            case RuntimePlatform.Android:
              Client = new AndroidClient(new AndroidConfiguration(apiKey));
              break;
            default:
              // this doesn't work on windows due to tls limitations in mono
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
