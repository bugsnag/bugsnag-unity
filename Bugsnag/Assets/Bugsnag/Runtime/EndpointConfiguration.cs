using System;
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BugsnagUnityTests")]

namespace BugsnagUnity
{
    public class EndpointConfiguration
    {
        // Base domain for secondary (previously InsightHub) endpoints
        private const string SECONDARY_URL_FORMAT = "bugsnag.smartbear.com";
        private const string DEFAULT_NOTIFY_ENDPOINT = "https://notify.bugsnag.com";
        private const string DEFAULT_SESSION_ENDPOINT = "https://sessions.bugsnag.com";
        private const string SECONDARY_NOTIFY_ENDPOINT = "https://notify." + SECONDARY_URL_FORMAT;
        private const string SECONDARY_SESSION_ENDPOINT = "https://sessions." + SECONDARY_URL_FORMAT;
        private const string SECONDARY_API_PREFIX = "00000"; // previously HubApiPrefix
        private string _customNotifyEndpoint = string.Empty;
        private string _customSessionEndpoint = string.Empty;
        internal Uri NotifyEndpoint;
        internal Uri SessionEndpoint;
        internal bool IsConfigured = false;

        internal void Configure(string apiKey)
        {
            if (IsConfigured || string.IsNullOrEmpty(apiKey))
            {
                return;
            }
            // Check that if one endpoint is customised the other is also customised
            if (!string.IsNullOrEmpty(_customNotifyEndpoint) && string.IsNullOrEmpty(_customSessionEndpoint))
            {
                UnityEngine.Debug.LogWarning("Invalid configuration. endpoints.NotifyEndpoint cannot be set without also setting endpoints.SessionEndpoint. Events will not be sent.");
                return;
            }
            if (!string.IsNullOrEmpty(_customSessionEndpoint) && string.IsNullOrEmpty(_customNotifyEndpoint))
            {
                UnityEngine.Debug.LogWarning("Invalid configuration. endpoints.SessionEndpoint cannot be set without also setting endpoints.NotifyEndpoint. Sessions will not be sent.");
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
                    NotifyEndpoint = new Uri(SECONDARY_NOTIFY_ENDPOINT);
                }
                else
                {
                    NotifyEndpoint = new Uri(DEFAULT_NOTIFY_ENDPOINT);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning(string.Format("Invalid configuration. Endpoints.NotifyEndpoint should be a valid URI. Error message: {0}. Events will not be sent. ", e.Message));
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(_customSessionEndpoint))
                {
                    SessionEndpoint = new Uri(_customSessionEndpoint);
                }
                else if (IsSecondaryApiKey(apiKey))
                {
                    SessionEndpoint = new Uri(SECONDARY_SESSION_ENDPOINT);
                }
                else
                {
                    SessionEndpoint = new Uri(DEFAULT_SESSION_ENDPOINT);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning(string.Format("Invalid configuration. Endpoints.SessionEndpoint should be a valid URI. Error message:  {0}. Sessions will not be sent. ", e.Message));
                return;
            }
            IsConfigured = true;
        }

        private bool IsSecondaryApiKey(string apiKey)
        {
            return apiKey.StartsWith(SECONDARY_API_PREFIX);
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
