using System;
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BugsnagUnityTests")]

namespace BugsnagUnity
{
    public class EndpointConfiguration
    {
        private const string DefaultNotifyEndpoint = "https://notify.bugsnag.com";
        private const string AlternateNotifyEndpoint = "https://notify.insighthub.smartbear.com";
        private const string DefaultSessionEndpoint = "https://sessions.bugsnag.com";
        private const string AlternateSessionEndpoint = "https://sessions.insighthub.smartbear.com";
        private string _customNotifyEndpoint = string.Empty;
        private string _customSessionEndpoint = string.Empty;
        internal Uri NotifyEndpoint;
        internal Uri SessionEndpoint;
        internal bool IsConfigured = false;

        internal void Configure(string apiKey)
        {
            if (IsConfigured)
            {
                return;
            }
            // Check that if one endpoint is customised the other is also customised
            if (!string.IsNullOrEmpty(_customNotifyEndpoint) && string.IsNullOrEmpty(_customSessionEndpoint))
            {
                UnityEngine.Debug.LogWarning("Invalid configuration. endpoints.Notify cannot be set without also setting endpoints.Session. Events will not be sent to Bugsnag.");
                return;
            }
            if (!string.IsNullOrEmpty(_customSessionEndpoint) && string.IsNullOrEmpty(_customNotifyEndpoint))
            {
                UnityEngine.Debug.LogWarning("Invalid configuration. endpoints.Session cannot be set without also setting endpoints.Notify. Sessions will not be sent to Bugsnag.");
                return;
            }
            try
            {
                if (!string.IsNullOrEmpty(_customNotifyEndpoint))
                {
                    NotifyEndpoint = new Uri(_customNotifyEndpoint);
                }
                else if (apiKey.StartsWith("00000"))
                {
                    NotifyEndpoint = new Uri(AlternateNotifyEndpoint);
                }
                else
                {
                    NotifyEndpoint = new Uri(DefaultNotifyEndpoint);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning(string.Format("Invalid configuration. Endpoints.Notify should be a valid URI. Error message: {0}. Events will not be sent to Bugsnag. ", e.Message));
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(_customSessionEndpoint))
                {
                    SessionEndpoint = new Uri(_customSessionEndpoint);
                }
                else if (apiKey.StartsWith("00000"))
                {
                    SessionEndpoint = new Uri(AlternateSessionEndpoint);
                }
                else
                {
                    SessionEndpoint = new Uri(DefaultSessionEndpoint);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning(string.Format("Invalid configuration. Endpoints.Session should be a valid URI. Error message:  {0}. Sessions will not be sent to Bugsnag. ", e.Message));
                return;
            }
            IsConfigured = true;
        }

        public EndpointConfiguration()
        {
        }

        public EndpointConfiguration(string notifyEndpoint, string sessionEndpoint)
        {
            _customNotifyEndpoint = notifyEndpoint;
            _customSessionEndpoint = sessionEndpoint;
        }

        internal EndpointConfiguration Clone()
        {
            return (EndpointConfiguration)MemberwiseClone();
        }

    }
}
