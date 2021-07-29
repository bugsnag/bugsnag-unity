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
        /// Adds session data to native client reports
        /// </summary>
        void SetSession(Session session);

        /// <summary>
        /// Adds user data to native client reports
        /// </summary>
        void SetUser(User user);

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

        /// <summary>
        /// Mutates the context.
        /// </summary>
        /// <param name="context"></param>
        void SetContext(string context);

        /// <summary>
        /// Mutates autoNotify.
        /// </summary>
        /// <param name="autoNotify"></param>
        void SetAutoDetectErrors(bool autoDetectErrors);

        /// <summary>
        /// Enables or disables Anr detection.
        /// </summary>
        /// <param name="autoDetectAnrs"></param>
        void SetAutoDetectAnrs(bool autoDetectAnrs);

    }
}
