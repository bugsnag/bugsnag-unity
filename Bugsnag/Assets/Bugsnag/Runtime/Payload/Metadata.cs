using System.Collections.Generic;
using System.Linq;
using BugsnagUnity;
using UnityEngine;

namespace BugsnagUnity.Payload
{
    public class Metadata : PayloadContainer
    {
        private INativeClient _nativeClient = null;

        public Metadata()
        {
        }

        internal Metadata(INativeClient nativeClient)
        {
            _nativeClient = nativeClient;
        }

        /// <summary>
        /// Sanitizes metadata values to prevent JSON serialization errors.
        /// Converts large unsigned 64-bit integers (> Int64.MaxValue) to strings,
        /// as they cannot be represented in standard JSON numeric format.
        /// </summary>
        private static object SanitizeValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            // Convert large ulongs to strings (> Int64.MaxValue causes JSON errors)
            if (value is ulong ulongValue && ulongValue > long.MaxValue)
            {
                return ulongValue.ToString();
            }

            // Recursively sanitize dictionaries
            if (value is IDictionary<string, object> dict)
            {
                var sanitized = new Dictionary<string, object>();
                foreach (var kvp in dict)
                {
                    sanitized[kvp.Key] = SanitizeValue(kvp.Value);
                }
                return sanitized;
            }

            // Recursively sanitize arrays/lists - preserve type for string truncation
            if (value is string[] stringArray)
            {
                var sanitized = new string[stringArray.Length];
                for (int i = 0; i < stringArray.Length; i++)
                {
                    var sanitizedValue = SanitizeValue(stringArray[i]);
                    sanitized[i] = sanitizedValue as string ?? sanitizedValue?.ToString();
                }
                return sanitized;
            }

            if (value is System.Collections.Generic.List<string> stringList)
            {
                var sanitized = new System.Collections.Generic.List<string>(stringList.Count);
                for (int i = 0; i < stringList.Count; i++)
                {
                    var sanitizedValue = SanitizeValue(stringList[i]);
                    sanitized.Add(sanitizedValue as string ?? sanitizedValue?.ToString());
                }
                return sanitized;
            }

            // Generic arrays/lists (preserving as object[] for mixed types)
            if (value is System.Collections.IList list)
            {
                var sanitized = new object[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    sanitized[i] = SanitizeValue(list[i]);
                }
                return sanitized;
            }

            return value;
        }


        public void AddMetadata(string section, string key, object value)
        {
            AddMetadata(section, new Dictionary<string, object> { { key, SanitizeValue(value) } });
        }

        public void AddMetadata(string section, IDictionary<string, object> metadataSection)
        {
            if (metadataSection == null)
            {
                ClearMetadata(section);
                return;
            }

            // Sanitize all values before storing
            var sanitizedSection = new Dictionary<string, object>();
            foreach (var kvp in metadataSection)
            {
                sanitizedSection[kvp.Key] = SanitizeValue(kvp.Value);
            }

            if (SectionExists(section))
            {
                var existingSection = (IDictionary<string, object>)Get(section);

                foreach (var key in sanitizedSection.Keys.ToList())
                {
                    var value = sanitizedSection[key];
                    if (value == null)
                    {
                        ClearMetadata(section, key);
                    }
                    else
                    {
                        existingSection[key] = value;
                    }
                }
            }
            else
            {
                Add(section, sanitizedSection);
            }
            if (_nativeClient != null)
            {
                _nativeClient.AddNativeMetadata(section, sanitizedSection);
            }
        }



        public void ClearMetadata(string section)
        {
            if (SectionExists(section))
            {
                Payload.Remove(section);
            }
            if (_nativeClient != null)
            {
                _nativeClient.ClearNativeMetadata(section);
            }
        }

        public void ClearMetadata(string section, string key)
        {
            if (SectionExists(section))
            {
                var existingSection = (IDictionary<string, object>)Payload[section];
                if (existingSection.ContainsKey(key))
                {
                    existingSection.Remove(key);
                }
            }
            if (_nativeClient != null)
            {
                _nativeClient.ClearNativeMetadata(section, key);
            }
        }

        private bool SectionExists(string section)
        {
            return Payload.ContainsKey(section);
        }

        public IDictionary<string, object> GetMetadata(string section)
        {
            return SectionExists(section) ? (IDictionary<string, object>)Payload[section] : null;
        }

        public object GetMetadata(string section, string key)
        {
            if (SectionExists(section))
            {
                var existingSection = (IDictionary<string, object>)Payload[section];
                return existingSection[key];
            }
            return null;
        }

        internal void MergeMetadata(IDictionary<string, object> newMetadata)
        {
            if (newMetadata == null)
            {
                return;
            }
            foreach (var section in newMetadata)
            {
                var sectionkey = section.Key;
                var sectionToMergeIn = (IDictionary<string, object>)section.Value;
                AddMetadata(sectionkey, sectionToMergeIn);
            }
        }

    }
}
