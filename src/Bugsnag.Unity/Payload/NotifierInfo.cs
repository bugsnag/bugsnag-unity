using Bugsnag.Unity;
using System.Collections.Generic;

namespace Bugsnag.Unity.Payload
{
  public class NotifierInfo : Dictionary<string, string>
  {
    public static NotifierInfo Instance { get; } = new NotifierInfo {
      { "name", "Unity Bugsnag Notifier" },
      { "version", typeof(Client).Assembly.GetName().Version.ToString(3) },
      { "url", "https://github.com/bugsnag/bugsnag-unity" }
    };
  }
}
