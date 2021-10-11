using System.Collections.Generic;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public delegate bool OnErrorCallback(Event bugsnagEvent);

    public delegate bool OnSessionCallback(Session session);


    public interface IClient
    {
        IConfiguration Configuration { get; }

        IBreadcrumbs Breadcrumbs { get; }

        ISessionTracker SessionTracking { get; }

        LastRunInfo LastRunInfo { get; }

        User User { get; }

        void Send(IPayload payload);

        void Notify(System.Exception exception);

        void Notify(System.Exception exception, OnErrorCallback callback);

        void Notify(System.Exception exception, Severity severity);

        void Notify(System.Exception exception, Severity severity, OnErrorCallback callback);

        void Notify(System.Exception exception, string stacktrace, OnErrorCallback callback);

        void Notify(string name, string message, string stackTrace, OnErrorCallback callback);

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

        void AddOnError(OnErrorCallback bugsnagCallback);

        void RemoveOnError(OnErrorCallback bugsnagCallback);

        void AddOnSession(OnSessionCallback callback);

        void RemoveOnSession(OnSessionCallback callback);

        void AddMetadata(string section, string key, object value);

        void AddMetadata(string section, Dictionary<string, object> metadata);

        void ClearMetadata(string section);

        void ClearMetadata(string section, string key);

        object GetMetadata(string section);

        object GetMetadata(string section, string key);

    }
}
