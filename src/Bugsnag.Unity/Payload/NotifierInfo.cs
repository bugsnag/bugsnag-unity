using System.Collections.Generic;

namespace Bugsnag.Unity.Payload
{
  class NotifierInfo : Dictionary<string, string>
  {
    internal static NotifierInfo Instance { get; } = new NotifierInfo {
      { "name", "Unity Bugsnag Notifier" },
      { "version", typeof(Client).Assembly.GetName().Version.ToString(3) },
      { "url", "https://github.com/bugsnag/bugsnag-unity" }
    };
  }
}
