using System;
using System.Collections.Generic;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{

    internal interface IClient : IMetadataEditor, IFeatureFlagStore
    {
        Configuration Configuration { get; }

        IBreadcrumbs Breadcrumbs { get; }

        ISessionTracker SessionTracking { get; }

        LastRunInfo LastRunInfo { get; }

        void Send(IPayload payload);

        void Notify(System.Exception exception, Func<IEvent, bool> callback);

        void Notify(System.Exception exception, Severity severity, Func<IEvent, bool> callback);

        void Notify(System.Exception exception, string stacktrace, Func<IEvent, bool> callback);

        void Notify(string name, string message, string stackTrace, Func<IEvent, bool> callback);

        /// <summary>
        /// Used to signal to the Bugsnag client that the focused state of the
        /// application has changed. This is used for session tracking and also
        /// the tracking of in foreground time for the application.
        /// </summary>
        /// <param name="inFocus"></param>
        void SetApplicationState(bool inFocus);

        string GetContext();

        void SetContext(string context);

        bool IsUsingFallback();

        void MarkLaunchCompleted();

        void AddOnError(Func<IEvent, bool> callback);

        void RemoveOnError(Func<IEvent, bool> callback);

        void AddOnSession(Func<ISession, bool> callback);

        void RemoveOnSession(Func<ISession, bool> callback);

        User GetUser();

        void SetUser(string id, string email, string name);

    }
}
