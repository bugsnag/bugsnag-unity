using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public delegate void Middleware(Report report);

    public interface IClient
    {
        IConfiguration Configuration { get; }

        IBreadcrumbs Breadcrumbs { get; }

        ISessionTracker SessionTracking { get; }

        User User { get; }

        void Send(IPayload payload);

        Metadata Metadata { get; }

        void BeforeNotify(Middleware middleware);

        void Notify(System.Exception exception);

        void Notify(System.Exception exception, Middleware callback);

        void Notify(System.Exception exception, Severity severity);

        void Notify(System.Exception exception, Severity severity, Middleware callback);

        void Notify(System.Exception exception, string stacktrace, Middleware callback);

        void Notify(string name, string message, string stackTrace, Middleware callback);

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
    }
}
