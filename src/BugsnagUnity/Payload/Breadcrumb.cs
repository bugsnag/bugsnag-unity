using System;
using System.Collections.Generic;
using System.Linq;

namespace BugsnagUnity.Payload
{
    /// <summary>
    /// Represents an individual breadcrumb in the error report payload.
    /// </summary>
    public class Breadcrumb : PayloadContainer
    {
        private const string UNDEFINED_NAME = "Breadcrumb";
        private const string NAME_KEY = "name";
        private const string MESSAGE_KEY = "message";
        private const string TIMESTAMP_KEY = "timestamp";
        private const string METADATA_KEY = "metaData";
        private const string TYPE_KEY = "type";

        internal static Breadcrumb FromReport(Report report)
        {
            var name = "Error";
            var metadata = new Dictionary<string, object> { };
            if (report.Context != null)
            {
                metadata["context"] = report.Context;
            }

            if (report.Exceptions != null && report.Exceptions.Any())
            {
                var exception = report.Exceptions.First();
                name = exception.ErrorClass;
                metadata.Add("message", exception.ErrorMessage);
            }

            return new Breadcrumb(name, metadata, BreadcrumbType.Error);
        }

        /// <summary>
        /// Used to construct a breadcrumb from the native data obtained from a
        /// native notifier if present.
        /// </summary>
        internal Breadcrumb(string name, string timestamp, string type, Dictionary<string, object> metadata)
        {
            Name = name;
            Timestamp = DateTime.Parse( timestamp );
            Metadata = metadata;
            Type = type;
        }

        internal Breadcrumb(string name, Dictionary<string, object> metadata, BreadcrumbType type)
        {
            if (name == null)
            {
                name = UNDEFINED_NAME;
            }
            Name = name;
            Timestamp = DateTime.UtcNow;
            Metadata = metadata;

            string breadcrumbType = Enum.GetName(typeof(BreadcrumbType), type).ToLower();
            if (string.IsNullOrEmpty(breadcrumbType))
            {
                breadcrumbType = "manual";
            }

            Type = breadcrumbType;
        }

        public Dictionary<string, object> Metadata
        {
            get
            {
                return Get(METADATA_KEY) as Dictionary<string, object>;
            }
            set
            {
                Add(METADATA_KEY, value);
            }
        }

        public string Name {
            get { return Get(NAME_KEY) as string; }
            set { Add(NAME_KEY,value); }
        }

        public string Message
        {
            get { return Get(MESSAGE_KEY) as string; }
            set { Add(MESSAGE_KEY, value); }
        }

        public string Type {
            get { return Get(TYPE_KEY) as string; }
            set { Add(TYPE_KEY, value); }
        }

        public DateTime Timestamp {
            get { return (DateTime)Get(TIMESTAMP_KEY); }
            set { Add(TIMESTAMP_KEY,value); }
        }

    }
}
