using System.Collections.Generic;

namespace Bugsnag.Unity.Native
{
  static class Client
  {
    internal static void Register(string apiKey, Dictionary<string, string> unityMetadata) { }

    internal static void Register(string apiKey, bool trackSessions, Dictionary<string, string> unityMetadata) { }

    internal static void SetNotifyUrl(string notifyUrl) { }

    internal static void SetAutoNotify(bool autoNotify) { }

    internal static void SetContext(string context) { }

    internal static void SetReleaseStage(string releaseStage) { }

    internal static void SetNotifyReleaseStages(string[] releaseStages) { }

    internal static void AddToTab(string tabName, string attributeName, string attributeValue) { }

    internal static void ClearTab(string tabName) { }

    internal static void SetBreadcrumbCapacity(int capacity) { }

    internal static void SetAppVersion(string version) { }

    internal static void SetUser(string id, string name, string email) { }
  }
}
