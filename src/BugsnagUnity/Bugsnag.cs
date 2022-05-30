using System;
using System.Collections.Generic;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public static class Bugsnag
    {

        private const string INIT_WARNING = "Bugsnag is already running and this call to Start() will be ignored. If this was unexpected please check whether Bugsnag is set to start automatically via the settings dialog.";

        static object _clientLock = new object();

        public static void Start(string apiKey)
        {
            Start(new Configuration(apiKey));
        }

        public static void Start(Configuration configuration)
        {

            lock (_clientLock)
            {
                if (InternalClient == null)
                {
                    var configClone = configuration.Clone();
                    var nativeClient = new NativeClient(configClone);
                    InternalClient = new Client(nativeClient);
                }
                else
                {
                    UnityEngine.Debug.LogWarning(INIT_WARNING);
                }
            }
        }

        private static Client InternalClient { get; set; }

        private static IClient Client => InternalClient;

        public static void Notify(string name, string message, string stackTrace) => InternalClient.Notify(name, message, stackTrace, null);

        public static void Notify(string name, string message, string stackTrace, Func<IEvent, bool> callback) => InternalClient.Notify(name, message, stackTrace, callback);

        public static void Notify(System.Exception exception) => InternalClient.Notify(exception, 3);

        public static void Notify(System.Exception exception, string stacktrace) => InternalClient.Notify(exception, stacktrace, null);

        public static void Notify(System.Exception exception, string stacktrace, Func<IEvent, bool> callback) => InternalClient.Notify(exception, stacktrace, callback);

        public static void Notify(System.Exception exception, Func<IEvent, bool> callback) => InternalClient.Notify(exception, callback, 3);

        public static void Notify(System.Exception exception, Severity severity) => InternalClient.Notify(exception, severity, 3);

        public static void Notify(System.Exception exception, Severity severity, Func<IEvent, bool> callback) => InternalClient.Notify(exception, severity, callback, 3);

        public static List<Breadcrumb> Breadcrumbs => Client.Breadcrumbs.Retrieve();

        public static void LeaveBreadcrumb(string message, Dictionary<string, object> metadata = null, BreadcrumbType type = BreadcrumbType.Manual ) => InternalClient.Breadcrumbs.Leave(message, metadata, type);

        public static User GetUser() => Client.GetUser();

        public static void SetUser(string id, string email, string name) => Client.SetUser(id, email, name);

        public static void StartSession() => InternalClient.SessionTracking.StartSession();

        public static void PauseSession() => InternalClient.SessionTracking.PauseSession();

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
        /// Setting Configuration.LaunchDurationMillis to 0 will cause Bugsnag to consider the app to be launching until Bugsnag.MarkLaunchCompleted() has been called.
        /// </summary>
        public static void MarkLaunchCompleted() => Client.MarkLaunchCompleted();
      
        /// <summary>
        /// Get information regarding the last application run. This will be null on non mobile platforms.
        /// </summary>
        public static LastRunInfo GetLastRunInfo() => Client.LastRunInfo;
       

        /// <summary>
        /// Add an OnError callback to run when an error occurs
        /// </summary>
        public static void AddOnError(Func<IEvent, bool> bugsnagCallback) => Client.AddOnError(bugsnagCallback);

        /// <summary>
        /// Remove an OnError callback
        /// </summary>
        public static void RemoveOnError(Func<IEvent, bool> bugsnagCallback) => Client.RemoveOnError(bugsnagCallback);
       
        /// <summary>
        /// Add an OnSession callback to run when an session is created
        /// </summary>
        public static void AddOnSession(Func<ISession, bool> callback) => Client.AddOnSession(callback);
       
        /// <summary>
        /// Remove an OnSession callback
        /// </summary>
        public static void RemoveOnSession(Func<ISession, bool> callback) => Client.RemoveOnSession(callback);

        /// <summary>
        /// AddMetadata that will appear in every reported event
        /// </summary>
        public static void AddMetadata(string section, string key, object value) => Client.AddMetadata(section, key, value);

        /// <summary>
        /// AddMetadata that will appear in every reported event
        /// </summary>
        public static void AddMetadata(string section, IDictionary<string, object> metadata) => Client.AddMetadata(section, metadata);

        /// <summary>
        /// Clear the metadata stored in the specified section
        /// </summary>
        public static void ClearMetadata(string section) => Client.ClearMetadata(section);

        /// <summary>
        /// Clear the metadata stored with the specified section and key
        /// </summary>
        public static void ClearMetadata(string section, string key) => Client.ClearMetadata(section, key);

        /// <summary>
        /// Get the metadata stored in the specified section 
        /// </summary>
        public static IDictionary<string, object> GetMetadata(string section) => Client.GetMetadata(section);

        /// <summary>
        /// Get the metadata stored with the specified section and key
        /// </summary>
        public static object GetMetadata(string section, string key) => Client.GetMetadata(section, key);

        public static void AddFeatureFlag(string name, string variant = null) => Client.AddFeatureFlag(name,variant);

        public static void AddFeatureFlags(FeatureFlag[] featureFlags) => Client.AddFeatureFlags(featureFlags);

        public static void ClearFeatureFlag(string name) => Client.ClearFeatureFlag(name);

        public static void ClearFeatureFlags() => Client.ClearFeatureFlags();

    }
}
