using System;
using System.Collections.Generic;
using System.Linq;

namespace BugsnagUnity.Payload
{
  /// <summary>
  /// Represents an individual breadcrumb in the error report payload.
  /// </summary>
  public class Breadcrumb : Dictionary<string, object>
  {
    private const int MaximumNameLength = 30;
    private const string UndefinedName = "Breadcrumb";
    private const int MaximumMetadataCharacterCount = 1024;

    internal static Breadcrumb FromReport(Report report)
    {
      var name = "Error";

      if (report.Event.OriginalSeverity != null)
      {
        name = report.Event.OriginalSeverity.Severity.ToString();
      }

      var metadata = new Dictionary<string, string>
      {
        { "context", report.Event.Context },
      };

      if (report.Event.Exceptions != null && report.Event.Exceptions.Any())
      {
        var exception = report.Event.Exceptions.First();

        metadata.Add("class", exception.ErrorClass);
        metadata.Add("message", exception.ErrorMessage);
      }

      return new Breadcrumb(name, BreadcrumbType.Error, metadata);
    }

    /// <summary>
    /// Used to construct a breadcrumb from the native data obtained from a
    /// native notifier if present.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="timestamp"></param>
    /// <param name="type"></param>
    /// <param name="metadata"></param>
    internal Breadcrumb(string name, string timestamp, string type, IDictionary<string, string> metadata)
    {
      this.AddToPayload("name", name);
      this.AddToPayload("timestamp", timestamp);
      this.AddToPayload("metaData", metadata);
      this.AddToPayload("type", type);
    }

    public Breadcrumb(string name, BreadcrumbType type) : this(name, type, null)
    {

    }

    public Breadcrumb(string name, BreadcrumbType type, IDictionary<string, string> metadata)
    {
      if (name == null) name = UndefinedName;
      if (name.Length > MaximumNameLength) name = name.Substring(0, MaximumNameLength);

      this.AddToPayload("name", name);
      this.AddToPayload("timestamp", DateTime.UtcNow);
      this.AddToPayload("metaData", metadata);

      string breadcrumbType;

      switch (type)
      {
        case BreadcrumbType.Navigation:
          breadcrumbType = "navigation";
          break;
        case BreadcrumbType.Request:
          breadcrumbType = "request";
          break;
        case BreadcrumbType.Process:
          breadcrumbType = "process";
          break;
        case BreadcrumbType.Log:
          breadcrumbType = "log";
          break;
        case BreadcrumbType.User:
          breadcrumbType = "user";
          break;
        case BreadcrumbType.State:
          breadcrumbType = "state";
          break;
        case BreadcrumbType.Error:
          breadcrumbType = "error";
          break;
        case BreadcrumbType.Manual:
        default:
          breadcrumbType = "manual";
          break;
      }

      this.AddToPayload("type", breadcrumbType);
    }

    public string Name { get { return this.Get("name") as string; } }

    public string Type { get { return this.Get("type") as string; } }

    public IDictionary<string, string> Metadata { get { return this.Get("metaData") as IDictionary<string, string>; } }
  }
}
