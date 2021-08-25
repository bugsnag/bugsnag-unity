using System;
using System.Collections.Generic;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public static class Bugsnag
    {
        static object _clientLock = new object();

        public static IClient Start(string apiKey)
        {
            return Start(new Configuration(apiKey));
        }

        public static IClient Start(IConfiguration configuration)
        {
            lock (_clientLock)
            {
                if (InternalClient == null)
                {
                    var nativeClient = new NativeClient(configuration);
                    InternalClient = new Client(nativeClient);
                }
            }

            return Client;
        }

        static Client InternalClient { get; set; }

        private static IClient Client => InternalClient;

        public static IBreadcrumbs Breadcrumbs => Client.Breadcrumbs;

        public static ISessionTracker SessionTracking => Client.SessionTracking;

        public static User User => Client.User;

        public static void Send(IPayload payload) => Client.Send(payload);

        public static Metadata Metadata => Client.Metadata;

        public static void BeforeNotify(Middleware middleware) => Client.BeforeNotify(middleware);

        public static void Notify(System.Exception exception) => InternalClient.Notify(exception, 3);

        public static void Notify(System.Exception exception, Middleware callback) => InternalClient.Notify(exception, callback, 3);

        public static void Notify(System.Exception exception, Severity severity) => InternalClient.Notify(exception, severity, 3);

        public static void Notify(System.Exception exception, Severity severity, Middleware callback) => InternalClient.Notify(exception, severity, callback, 3);

        public static void LeaveBreadcrumb(string message) => InternalClient.Breadcrumbs.Leave(message);

        public static void LeaveBreadcrumb(string message, BreadcrumbType type, IDictionary<string, string> metadata) => InternalClient.Breadcrumbs.Leave(message, type, metadata);

        public static void LeaveBreadcrumb(Breadcrumb breadcrumb) => InternalClient.Breadcrumbs.Leave(breadcrumb);

        public static void SetUser(string id, string email, string name)
        {
            User.Id = id;
            User.Email = email;
            User.Name = name;
        }

        public static void StartSession() => InternalClient.SessionTracking.StartSession();

        public static void StopSession() => InternalClient.SessionTracking.StopSession();

        public static bool ResumeSession() => InternalClient.SessionTracking.ResumeSession();

        /// <summary>
        /// Used to signal to the Bugsnag client that the focused state of the
        /// application has changed. This is used for session tracking and also
        /// the tracking of in foreground time for the application.
        /// </summary>
        /// <param name="inFocus"></param>
        public static void SetApplicationState(bool inFocus) => Client.SetApplicationState(inFocus);

        /// <summary>
        /// Bugsnag uses the concept of contexts to help display and group your errors.
        /// Contexts represent what was happening in your game at the time an error
        /// occurs. Unless manually set, this will be automatically set to be your currently active Unity Scene.
        /// </summary>
        /// <param name="context"></param>
        [Obsolete("SetContext is deprecated, please use the property Bugsnag.Context instead.", false)]
        public static void SetContext(string context)
        {
            Client.SetContext(context);
        }

        /// <summary>
        /// Bugsnag uses the concept of contexts to help display and group your errors.
        /// Contexts represent what was happening in your game at the time an error
        /// occurs. Unless manually set, this will be automatically set to be your currently active Unity Scene.
        /// </summary>
        public static string Context
        {
            get
            {
                return Client.GetContext();
            }
            set
            {
                Client.SetContext(value);
            }
        }

        /// <summary>
        /// By default, we will automatically notify Bugsnag of any fatal errors (crashes) in your game.
        /// If you want to stop this from happening, you can set the AutoNotify property to false. It
        /// is recommended that you set this value by Configuration at init rather than this method.
        /// </summary>
        /// <param name="autoNotify"></param>
        [Obsolete("SetAutoNotify is deprecated, please use SetAutoDetectErrors instead.", false)]
        public static void SetAutoNotify(bool autoNotify)
        {
            SetAutoDetectErrors(autoNotify);
        }

        /// <summary>
        /// By default, we will automatically notify Bugsnag of any fatal errors (crashes) in your game.
        /// If you want to stop this from happening, you can set the AutoDetectErrors property to false. It
        /// is recommended that you set this value by Configuration at init rather than this method.
        /// </summary>
        /// <param name="autoDetectErrors"></param>
        public static void SetAutoDetectErrors(bool autoDetectErrors)
        {
            Client.SetAutoDetectErrors(autoDetectErrors);
        }

        /// <summary>
        /// Enable or disable Bugsnag reporting any Android not responding errors (ANRs) in your game.
        /// </summary>
        /// <param name="autoDetectAnrs"></param>
        public static void SetAutoDetectAnrs(bool autoDetectAnrs)
        {
            Client.SetAutoDetectAnrs(autoDetectAnrs);
        }

    }
}
