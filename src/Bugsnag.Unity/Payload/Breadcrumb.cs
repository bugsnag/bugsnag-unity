using System;
using System.Collections.Generic;

namespace Bugsnag.Unity.Payload
{
  /// <summary>
  /// Represents an individual breadcrumb in the error report payload.
  /// </summary>
  public class Breadcrumb : Dictionary<string, object>
  {
    private const int MaximumNameLength = 30;
    private const string UndefinedName = "Breadcrumb";
    private const int MaximumMetadataCharacterCount = 1024;

    public Breadcrumb(Native.Breadcrumb breadcrumb)
    {
      this.AddToPayload("name", breadcrumb.Name);
      this.AddToPayload("timestamp", breadcrumb.Timestamp);
      this.AddToPayload("metaData", breadcrumb.Metadata);
      this.AddToPayload("type", breadcrumb.Type);
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

    public IDictionary<string, string> Metadata { get { return this.Get("metaData") as IDictionary<string, string>; } }
  }
}
