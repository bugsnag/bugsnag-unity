using System;
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BugsnagUnityTests")]

namespace BugsnagUnity
{
    public class EndpointConfiguration
    {
        private const string DefaultNotifyEndpoint = "https://notify.bugsnag.com";
        private const string HubNotifyEndpoint = "https://notify.insighthub.smartbear.com";
        private const string DefaultSessionEndpoint = "https://sessions.bugsnag.com";
        private const string HubSessionEndpoint = "https://sessions.insighthub.smartbear.com";
        private const string HubApiPrefix = "00000";
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
                    NotifyEndpoint = new Uri(HubNotifyEndpoint);
                }
                else
                {
                    NotifyEndpoint = new Uri(DefaultNotifyEndpoint);
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
                else if (IsHubApiKey(apiKey))
                {
                    SessionEndpoint = new Uri(HubSessionEndpoint);
                }
                else
                {
                    SessionEndpoint = new Uri(DefaultSessionEndpoint);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning(string.Format("Invalid configuration. Endpoints.SessionEndpoint should be a valid URI. Error message:  {0}. Sessions will not be sent. ", e.Message));
                return;
            }
            IsConfigured = true;
        }

        private bool IsHubApiKey(string apiKey)
        {
            return apiKey.StartsWith(HubApiPrefix);
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
