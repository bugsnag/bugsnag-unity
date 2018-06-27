using System.Collections.Generic;

namespace Bugsnag.Unity.Payload
{
  /// <summary>
  /// Represents the "app" key in the error report payload.
  /// </summary>
  class App : Dictionary<string, object>, IFilterable
  {
    internal App(IConfiguration configuration) : this(configuration.AppVersion, configuration.ReleaseStage, null)
    {

    }

    internal App(string version, string releaseStage, string type)
    {
      this.AddToPayload("version", version);
      this.AddToPayload("releaseStage", releaseStage);
      this.AddToPayload("type", type);
    }
  }
}
