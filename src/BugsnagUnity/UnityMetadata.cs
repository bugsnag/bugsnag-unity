using UnityEngine;
using System.Collections.Generic;

namespace BugsnagUnity
{
  class UnityMetadata
  {
    internal static Dictionary<string, string> ForNativeClient() => ForUnityException(false);

    internal static Dictionary<string, string> WithLogType(LogType? logType) {
      var data = ForUnityException(true);

      if (logType.HasValue)
      {
        data["unityLogType"] = logType.Value.ToString("G");
      }

      return data;
    }

    private static Dictionary<string, string> ForUnityException(bool unityException) => new Dictionary<string, string> {
      { "unityVersion", Application.unityVersion },
      { "unityException", unityException.ToString().ToLowerInvariant() },
      { "platform", Application.platform.ToString() },
      { "osLanguage", Application.systemLanguage.ToString() },
      { "version", Application.version },
      { "companyName", Application.companyName },
      { "productName", Application.productName },
    };
  }
}
