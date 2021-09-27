using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public delegate bool BugsnagCallback(Event bugsnagEvent);

    public interface IClient
    {
        IConfiguration Configuration { get; }

        IBreadcrumbs Breadcrumbs { get; }

        ISessionTracker SessionTracking { get; }

        LastRunInfo LastRunInfo { get; }

        User User { get; }

        void Send(IPayload payload);

        Metadata Metadata { get; }

        void Notify(System.Exception exception);

        void Notify(System.Exception exception, BugsnagCallback callback);

        void Notify(System.Exception exception, Severity severity);

        void Notify(System.Exception exception, Severity severity, BugsnagCallback callback);

        void Notify(System.Exception exception, string stacktrace, BugsnagCallback callback);

        void Notify(string name, string message, string stackTrace, BugsnagCallback callback);

        /// <summary>
        /// Used to signal to the Bugsnag client that the focused state of the
        /// application has changed. This is used for session tracking and also
        /// the tracking of in foreground time for the application.
        /// </summary>
        /// <param name="inFocus"></param>
        void SetApplicationState(bool inFocus);

        void SetContext(string context);

        string GetContext();

        void SetAutoDetectErrors(bool AutoDetectErrors);

        void SetAutoDetectAnrs(bool autoDetectAnrs);

        bool IsUsingFallback();

        void MarkLaunchCompleted();
    }
}
