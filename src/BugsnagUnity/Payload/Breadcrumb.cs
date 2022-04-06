using System;
using System.Collections.Generic;
using System.Linq;

namespace BugsnagUnity.Payload
{
    /// <summary>
    /// Represents an individual breadcrumb in the error report payload.
    /// </summary>
    public class Breadcrumb : PayloadContainer, IBreadcrumb
    {
        // Notifier spec specifies Message, but the pipeline is still expecting the legacy field name
        private const string MESSAGE_KEY = "name";
        private const string TIMESTAMP_KEY = "timestamp";
        private const string METADATA_KEY = "metaData";
        private const string TYPE_KEY = "type";

        internal static Breadcrumb FromReport(Report report)
        {
            var message = "Error";
            var metadata = new Dictionary<string, object> { };
            if (report.Context != null)
            {
                metadata["context"] = report.Context;
            }
            if (report.Exceptions != null && report.Exceptions.Any())
            {
                var exception = report.Exceptions.First();
                metadata["message"] = exception.ErrorMessage;
                metadata["errorClass"] = exception.ErrorClass;
                metadata["unhandled"] = !report.IsHandled;
                metadata["severity"] = report.OriginalSeverity;
                message = exception.ErrorClass;
            }
            return new Breadcrumb(message, metadata, BreadcrumbType.Error);
        }

        internal Breadcrumb(Dictionary<string, object> data)
        {
            Add(data);
        }

        /// <summary>
        /// Used to construct a breadcrumb from the native data obtained from a
        /// native notifier if present.
        /// </summary>
        internal Breadcrumb(string message, string timestamp, string type, IDictionary<string, object> metadata)
        {
            Timestamp = DateTimeOffset.Parse( timestamp );
            Metadata = metadata;
            if (string.IsNullOrEmpty(type))
            {
                Type = BreadcrumbType.Manual;
            }
            else
            {
                Type = ParseBreadcrumbType(type);
            }
            Message = message;
        }

        internal Breadcrumb(string message,IDictionary<string, object> metadata, BreadcrumbType type)
        {
            Timestamp = DateTime.UtcNow;
            Metadata = metadata;
            Type = type;
            Message = message;
        }

        public IDictionary<string, object> Metadata
        {
            get
            {
                if (Get(METADATA_KEY) == null)
                {
                    Metadata = new Dictionary<string, object>();
                }
                return Get(METADATA_KEY) as IDictionary<string, object>;
            }
            set
            {
                Add(METADATA_KEY, value);
            }
        }

        public string Message
        {
            get { return Get(MESSAGE_KEY) as string; }
            set { Add(MESSAGE_KEY, value); }
        }

        public BreadcrumbType Type {
            get {
                var stringValue = (string)Get(TYPE_KEY);
                return ParseBreadcrumbType(stringValue);
            }
            set { Add(TYPE_KEY, value.ToString().ToLowerInvariant()); }
        }

        public DateTimeOffset? Timestamp {
            get { return (DateTimeOffset)Get(TIMESTAMP_KEY); }
            set { Add(TIMESTAMP_KEY,value); }
        }

        internal static BreadcrumbType ParseBreadcrumbType(string name)
        {
            if (name.Contains("error"))
            {
                return BreadcrumbType.Error;
            }
            if (name.Contains("log"))
            {
                return BreadcrumbType.Log;
            }
            if (name.Contains("navigation"))
            {
                return BreadcrumbType.Navigation;
            }
            if (name.Contains("process"))
            {
                return BreadcrumbType.Process;
            }
            if (name.Contains("request"))
            {
                return BreadcrumbType.Request;
            }
            if (name.Contains("state"))
            {
                return BreadcrumbType.State;
            }
            if (name.Contains("user"))
            {
                return BreadcrumbType.User;
            }
            if (name.Contains("manual"))
            {
                return BreadcrumbType.Manual;
            }
            return BreadcrumbType.Manual;            
        }

    }
}
