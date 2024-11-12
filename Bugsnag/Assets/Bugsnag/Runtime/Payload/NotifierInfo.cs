using System.Collections.Generic;

namespace BugsnagUnity.Payload
{
    class NotifierInfo : Dictionary<string, string>
    {
        internal const string NotifierName = "Unity Bugsnag Notifier";
        internal static string NotifierVersion = typeof(Client).Assembly.GetName().Version.ToString(3);
        internal const string NotifierUrl = "https://github.com/bugsnag/bugsnag-unity";

        internal static NotifierInfo Instance { get; } = new NotifierInfo {
      { "name", NotifierName },
      { "version", NotifierVersion },
      { "url", NotifierUrl }
    };
    }
}
