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
        internal Breadcrumb(
            string message,
            string timestamp,
            string type,
            IDictionary<string, object> metadata
        )
        {
            Timestamp = DateTimeOffset.Parse(timestamp);
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

        internal Breadcrumb(
            string message,
            IDictionary<string, object> metadata,
            BreadcrumbType type
        )
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
                // Sanitize metadata to avoid JSON serializer reflection errors.
                if (value == null)
                {
                    Add(METADATA_KEY, null);
                    return;
                }

                var sanitized = new Dictionary<string, object>();
                var warnings = new List<string>();

                SanitizeAndCollectWarnings(value, sanitized, warnings, "");

                if (warnings.Count > 0)
                {
                    const string warnKey = "__bugsnag_unserializable_values";
                    if (sanitized.TryGetValue(warnKey, out var existing))
                    {
                        switch (existing)
                        {
                            case List<string> existingList:
                                existingList.AddRange(warnings);
                                sanitized[warnKey] = existingList.ToArray();
                                break;
                            case IEnumerable<object> existingEnum:
                                var merged = new List<string>();
                                foreach (var obj in existingEnum)
                                    merged.Add(obj?.ToString());
                                merged.AddRange(warnings);
                                sanitized[warnKey] = merged.ToArray();
                                break;
                            default:
                                sanitized[warnKey] = new List<string> { existing?.ToString() }
                                    .Concat(warnings)
                                    .ToArray();
                                break;
                        }
                    }
                    else
                    {
                        sanitized[warnKey] = warnings.ToArray();
                    }
                }

                Add(METADATA_KEY, sanitized);
            }
        }

        private static void SanitizeAndCollectWarnings(
            IDictionary<string, object> source,
            IDictionary<string, object> dest,
            List<string> warnings,
            string path
        )
        {
            foreach (var kvp in source)
            {
                var fullPath = string.IsNullOrEmpty(path) ? kvp.Key : $"{path}.{kvp.Key}";
                dest[kvp.Key] = SanitizeValueAndCollectWarnings(kvp.Value, warnings, fullPath);
            }
        }

        private static object SanitizeValueAndCollectWarnings(
            object value,
            List<string> warnings,
            string path
        )
        {
            // Check type of original value first to decide if we need to recurse
            // Recurse into nested dictionaries (use original, not sanitized)
            if (value is IDictionary<string, object> dictValue)
            {
                var sanitizedNested = new Dictionary<string, object>();
                SanitizeAndCollectWarnings(dictValue, sanitizedNested, warnings, path);
                return sanitizedNested;
            }

            // Recurse into collections (but not strings) - use original
            // Special case: don't recurse into string arrays/lists - preserve their type
            // exactly as-is for proper truncation in Delivery
            if (
                value is string[]
                || value is System.Collections.Generic.List<string>
                || value is System.Collections.Generic.Dictionary<string, string>
            )
            {
                return value;
            }

            if (value is System.Collections.IEnumerable enumValue && !(value is string))
            {
                // For other collections, use List<object>
                var sanitizedObjList = new List<object>();
                int index = 0;
                foreach (var item in enumValue)
                {
                    var itemPath = $"{path}[{index}]";
                    sanitizedObjList.Add(SanitizeValueAndCollectWarnings(item, warnings, itemPath));
                    index++;
                }
                return sanitizedObjList;
            }

            // For leaf values (strings, numbers, etc.), sanitize them
            var sVal = SanitizationHelpers.SanitizeValue(value);

            // Check if the value is unserializable
            if (!SanitizationHelpers.IsTriviallySerializable(sVal) && sVal != null)
            {
                var typeName = sVal.GetType().FullName;
                warnings.Add(
                    $"Could not serialize breadcrumb metadata key '{path}' (type: {typeName})"
                );
                return sVal.ToString();
            }

            return sVal;
        }

        public string Message
        {
            get { return Get(MESSAGE_KEY) as string; }
            set { Add(MESSAGE_KEY, value); }
        }

        public BreadcrumbType Type
        {
            get
            {
                var stringValue = (string)Get(TYPE_KEY);
                return ParseBreadcrumbType(stringValue);
            }
            set { Add(TYPE_KEY, value.ToString().ToLowerInvariant()); }
        }

        public DateTimeOffset? Timestamp
        {
            get { return (DateTimeOffset)Get(TIMESTAMP_KEY); }
            set { Add(TIMESTAMP_KEY, value); }
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
