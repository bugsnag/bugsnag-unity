using System;
namespace BugsnagUnity
{
    public class EndpointConfiguration
    {

        private const string DefaultNotifyEndpoint = "https://notify.bugsnag.com";

        private const string DefaultSessionEndpoint = "https://sessions.bugsnag.com";

        internal Uri Notify;

        internal Uri Session;

        internal bool NotifyIsCustom => Notify != null && Notify.ToString() != new Uri( DefaultNotifyEndpoint ).ToString();

        internal bool SessionIsCustom => Session != null && Session.ToString() != new Uri ( DefaultSessionEndpoint ).ToString();

        internal bool IsValid
        {
            get
            {
                return Notify != null && Session != null && (NotifyIsCustom && SessionIsCustom || !NotifyIsCustom && !SessionIsCustom);
            }
        }        

        internal EndpointConfiguration()
        {
            Notify = new Uri(DefaultNotifyEndpoint);
            Session = new Uri(DefaultSessionEndpoint);
        }

        public EndpointConfiguration(string notifyEndpoint, string sessionEndpoint)
        {
            try
            {
                Notify = new Uri(notifyEndpoint);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning( string.Format("Invalid configuration. Endpoints.Notify should be a valid URI. Error message: {0}. Events will not be sent to Bugsnag. " , e.Message));
            }
            try
            {
                Session = new Uri(sessionEndpoint);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning(string.Format("Invalid configuration. Endpoints.Session should be a valid URI. Error message:  {0}. Sessions will not be sent to Bugsnag. ", e.Message));

            }
        }
       
    }
}
