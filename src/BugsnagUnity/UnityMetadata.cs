using UnityEngine;
using System.Collections.Generic;

namespace BugsnagUnity
{
  class UnityMetadata
  {
    private static Dictionary<string, string> DefaultMetadata = new Dictionary<string, string>();

    internal static void InitDefaultMetadata() {
      if (DefaultMetadata.Count > 0) {
        return; // Already initialized
      }
      DefaultMetadata.Add("unityVersion", Application.unityVersion);
      DefaultMetadata.Add("osLanguage", Application.systemLanguage.ToString());
      DefaultMetadata.Add("platform", Application.platform.ToString());
      DefaultMetadata.Add("version", Application.version);
      DefaultMetadata.Add("companyName", Application.companyName);
      DefaultMetadata.Add("productName", Application.productName);
    }

    internal static Dictionary<string, string> ForNativeClient() => ForUnityException(false);

    internal static Dictionary<string, string> WithLogType(LogType? logType) {
      var data = ForUnityException(true);

      if (logType.HasValue)
      {
        data["unityLogType"] = logType.Value.ToString("G");
      }

      return data;
    }

    private static Dictionary<string, string> ForUnityException(bool unityException) {
      var metadata = new Dictionary<string, string> {
        { "unityException", unityException.ToString().ToLowerInvariant() },
      };

      foreach (var pair in DefaultMetadata) {
        metadata.Add(pair.Key, pair.Value);
      }

      return metadata;
    }
  }
}
