using BugsnagUnity.Payload;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace BugsnagUnity
{
    interface INativeClient : IFeatureFlagStore
    {
        /// <summary>
        /// The native configuration
        /// </summary>
        Configuration Configuration { get; }

        /// <summary>
        /// The native breadcrumbs
        /// </summary>
        IBreadcrumbs Breadcrumbs { get; }

        /// <summary>
        /// Populates the native app information
        /// </summary>
        /// <returns></returns>
        void PopulateApp(App app);

        /// <summary>
        /// Populates the native app information
        /// </summary>
        /// <returns></returns>
        void PopulateAppWithState(AppWithState app);

        /// <summary>
        /// Populates the native device information
        /// </summary>
        /// <returns></returns>
        void PopulateDevice(Device device);

        /// <summary>
        /// Populates the native device information
        /// </summary>
        /// <returns></returns>
        void PopulateDeviceWithState(DeviceWithState device);

        /// <summary>
        /// Send the start session message to a native notifier
        /// </summary>
        void StartSession();

        /// <summary>
        /// Send the stop session message to a native notifier
        /// </summary>
        void PauseSession();

        /// <summary>
        /// Send the resume session message to a native notifier
        /// </summary>
        bool ResumeSession();

        /// <summary>
        /// Update the current native session
        /// </summary>
        void UpdateSession(Session session);

        /// <summary>
        /// Get the current session info for sending with an error
        /// </summary>
        Session GetCurrentSession();

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
        /// Mutates the context.
        /// </summary>
        /// <param name="context"></param>
        void SetContext(string context);

        /// <summary>
        /// Setting Configuration.LaunchDurationMillis to 0 will cause Bugsnag to consider the app to be launching until Bugsnag.MarkLaunchCompleted() has been called.
        /// </summary>
        void MarkLaunchCompleted();

        /// <summary>
        /// Get the last run information from Android and cocoa platforms
        /// </summary>
        LastRunInfo GetLastRunInfo();

        void ClearNativeMetadata(string section);

        void ClearNativeMetadata(string section, string key);

        void AddNativeMetadata(string section, IDictionary<string, object> data);

        IDictionary<string, object> GetNativeMetadata();

        bool ShouldAttemptDelivery();

        void AddOnSession();

    }
}
