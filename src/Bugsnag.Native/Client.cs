using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bugsnag.Native
{
  public static class Client
  {
    public static void Register(string apiKey) { }

    public static void Register(string apiKey, bool trackSessions) { }

    public static void SetNotifyUrl(string notifyUrl) { }

    public static void SetSessionUrl(string sessionUrl) { }
  }
}
