using BugsnagUnity.Payload;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace BugsnagUnity
{
  interface INativeClient
  {
    /// <summary>
    /// The native configuration
    /// </summary>
    IConfiguration Configuration { get; }

    /// <summary>
    /// The native breadcrumbs
    /// </summary>
    IBreadcrumbs Breadcrumbs { get; }

    /// <summary>
    /// The native delivery method
    /// </summary>
    IDelivery Delivery { get; }

    /// <summary>
    /// Populates the native app information
    /// </summary>
    /// <returns></returns>
    void PopulateApp(App app);

    /// <summary>
    /// Populates the native device information
    /// </summary>
    /// <returns></returns>
    void PopulateDevice(Device device);

    /// <summary>
    /// Adds the metadata to the native clients metadata
    /// </summary>
    /// <param name="metadata"></param>
    void SetMetadata(string tab, Dictionary<string, string> metadata);

    /// <summary>
    /// Populates the native user information.
    /// </summary>
    /// <returns></returns>
    void PopulateUser(User user);

    /// <summary>
    /// Populates any native metadata.
    /// </summary>
    /// <param name="metadata"></param>
    void PopulateMetadata(Metadata metadata);
  }
}
