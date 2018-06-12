using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bugsnag.Native
{
  public static class Client
  {
    public static void Register(string apiKey, Dictionary<string, string> unityMetadata) { }

    public static void Register(string apiKey, bool trackSessions, Dictionary<string, string> unityMetadata) { }

    public static void SetNotifyUrl(string notifyUrl) { }

    public static void SetAutoNotify(bool autoNotify) { }

    public static void SetContext(string context) { }

    public static void SetReleaseStage(string releaseStage) { }

    public static void SetNotifyReleaseStages(string[] releaseStages) { }

    public static void AddToTab(string tabName, string attributeName, string attributeValue) { }

    public static void ClearTab(string tabName) { }

    public static void LeaveBreadcrumb(string breadcrumb) { }

    public static void SetBreadcrumbCapacity(int capacity) { }

    public static void SetAppVersion(string version) { }

    public static void SetUser(string id, string name, string email) { }

    public static Breadcrumb[] GetBreadcrumbs() => new Breadcrumb[0];
  }

  public class Breadcrumb
  {
    public string Name { get; set; }

    public string Type { get; set; }

    public Dictionary<string, string> Metadata { get; set; }

    public string Timestamp { get; set; }
  }
}
