using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity.Payload
{
    /// <summary>
    /// Represents the various fields that can be set in the "event" payload for
    /// showing the exceptions handled/unhandled state and severity.
    /// </summary>
    class HandledState : Dictionary<string, object>
    {
        /// <summary>
        /// Creates a HandledState object for an error report payload where the exception was not handled by the application
        /// and caught by a global error handler.
        /// </summary>
        /// <returns></returns>
        internal static HandledState ForUnhandledException()
        {
            return new HandledState(false, Severity.Error, SeverityReason.ForUnhandledException());
        }

        /// <summary>
        /// Creates a HandledState object for an error report payload where the exception was handled by the application
        /// and notified manually.
        /// </summary>
        /// <returns></returns>
        internal static HandledState ForHandledException()
        {
            return new HandledState(true, Severity.Warning, SeverityReason.ForHandledException());
        }

        /// <summary>
        /// Creates a HandledState object for an error report payload where the exception was handled by the application
        /// and notified manually and the error severity was also passed in to override the default severity.
        /// </summary>
        /// <param name="severity"></param>
        /// <returns></returns>
        internal static HandledState ForUserSpecifiedSeverity(Severity severity)
        {
            return new HandledState(true, severity, SeverityReason.ForUserSpecifiedSeverity());
        }

        /// <summary>
        /// Creates a HandledState object for an error report payload where the severity for the exception was modified
        /// while running the middleware/callback.
        /// </summary>
        /// <param name="severity"></param>
        /// <param name="previousSeverity"></param>
        /// <returns></returns>
        internal static HandledState ForCallbackSpecifiedSeverity(Severity severity, HandledState previousSeverity)
        {
            return new HandledState(previousSeverity.Handled, severity, SeverityReason.ForCallbackSpecifiedSeverity());
        }

        /// <summary>
        /// Creates a HandledState object for an error report payload that is being generated from a Unity log message.
        /// These are always attributed as a handled exception due to the fact that they do not crash the application.
        /// </summary>
        /// <returns>The unity log message.</returns>
        /// <param name="severity">Severity.</param>
        internal static HandledState ForUnityLogMessage(Severity severity)
        {
            return new HandledState(true, severity, SeverityReason.ForHandledException());
        }

        internal Severity Severity { get; }

        HandledState(bool handled, Severity severity, SeverityReason reason)
        {
            Severity = severity;
            this.AddToPayload("unhandled", !handled);
            this.AddToPayload("severityReason", reason);

            string severityValue;

            switch (severity)
            {
                case Severity.Info:
                    severityValue = "info";
                    break;
                case Severity.Warning:
                    severityValue = "warning";
                    break;
                default:
                    severityValue = "error";
                    break;
            }

            this.AddToPayload("severity", severityValue);
        }

        internal bool Handled
        {
            get
            {
                switch (this.Get("unhandled"))
                {
                    case bool unhandled:
                        return !unhandled;
                    default:
                        return true;
                }
            }
            set
            {
                this.AddToPayload("unhandled", !value);
            }
        }

        /// <summary>
        /// Represents the "severityReason" key in the error report payload.
        /// </summary>
        class SeverityReason : Dictionary<string, object>
        {
            internal static SeverityReason ForUnhandledException()
            {
                return new SeverityReason("unhandledException", null);
            }

            internal static SeverityReason ForHandledException()
            {
                return new SeverityReason("handledException", null);
            }

            internal static SeverityReason ForUserSpecifiedSeverity()
            {
                return new SeverityReason("userSpecifiedSeverity", null);
            }

            internal static SeverityReason ForCallbackSpecifiedSeverity()
            {
                return new SeverityReason("userCallbackSetSeverity", null);
            }

            SeverityReason(string type, IDictionary<string, string> attributes)
            {
                this.AddToPayload("type", type);
                this.AddToPayload("attributes", attributes);
            }
        }
    }
}
