using System;
using System.Collections.Generic;
using System.Linq;

namespace BugsnagUnity.Payload
{
    /// <summary>
    /// Represents an individual breadcrumb in the error report payload.
    /// </summary>
    public class Breadcrumb : Dictionary<string, object>
    {
        private const string UndefinedName = "Breadcrumb";

        internal static Breadcrumb FromReport(Report report)
        {
            var name = "Error";
            var metadata = new Dictionary<string, string> { };
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

            return new Breadcrumb(name, BreadcrumbType.Error, metadata);
        }

        /// <summary>
        /// Used to construct a breadcrumb from the native data obtained from a
        /// native notifier if present.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="timestamp"></param>
        /// <param name="type"></param>
        /// <param name="metadata"></param>
        internal Breadcrumb(string name, string timestamp, string type, IDictionary<string, string> metadata)
        {
            this.AddToPayload("name", name);
            this.AddToPayload("timestamp", timestamp);
            this.AddToPayload("metaData", metadata);
            this.AddToPayload("type", type);
        }

        public Breadcrumb(string name, BreadcrumbType type) : this(name, type, null)
        {

        }

        public Breadcrumb(string name, BreadcrumbType type, IDictionary<string, string> metadata)
        {
            if (name == null) name = UndefinedName;

            this.AddToPayload("name", name);
            this.AddToPayload("timestamp", DateTime.UtcNow);
            this.AddToPayload("metaData", metadata);

            string breadcrumbType = Enum.GetName(typeof(BreadcrumbType), type).ToLower();

            if (string.IsNullOrEmpty(breadcrumbType))
            {
                breadcrumbType = "manual";
            }

            this.AddToPayload("type", breadcrumbType);
        }

        public string Name { get { return this.Get("name") as string; } }

        public string Type { get { return this.Get("type") as string; } }

        public IDictionary<string, string> Metadata { get { return this.Get("metaData") as IDictionary<string, string>; } }
    }
}
