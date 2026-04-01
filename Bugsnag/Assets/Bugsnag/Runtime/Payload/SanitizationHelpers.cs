using System;
using System.Collections;
using System.Collections.Generic;

namespace BugsnagUnity.Payload
{
    internal static class SanitizationHelpers
    {
        internal static bool IsTriviallySerializable(object value)
        {
            switch (value)
            {
                case null:
                case string _:
                case bool _:
                case byte _:
                case sbyte _:
                case short _:
                case ushort _:
                case int _:
                case uint _:
                case long _:
                case ulong _:
                case float _:
                case double _:
                case decimal _:
                case DateTime _:
                case DateTimeOffset _:
                case Guid _:
                case Enum _:
                case IDictionary _:
                case IEnumerable _:
                    return true;
                default:
                    return false;
            }
        }

        internal static object SanitizeValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            // Convert large ulongs to strings (> Int64.MaxValue causes JSON errors)
            if (value is ulong ulongValue && ulongValue > long.MaxValue)
            {
                return ulongValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
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

            // Handle non-generic IDictionary (convert keys to strings for JSON compatibility)
            if (value is System.Collections.IDictionary nonGenericDict)
            {
                var sanitized = new Dictionary<string, object>();
                foreach (System.Collections.DictionaryEntry entry in nonGenericDict)
                {
                    var keyString = entry.Key is string str
                        ? str
                        : entry.Key?.ToString() ?? "null";
                    sanitized[keyString] = SanitizeValue(entry.Value);
                }
                return sanitized;
            }

            // Handle generic IDictionary<string, T> variants
            var valueType = value.GetType();
            foreach (var iface in valueType.GetInterfaces())
            {
                if (iface.IsGenericType &&
                    iface.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    var genericArgs = iface.GetGenericArguments();
                    if (genericArgs[0] == typeof(string))
                    {
                        var sanitized = new Dictionary<string, object>();
                        foreach (var item in (System.Collections.IEnumerable)value)
                        {
                            var itemType = item.GetType();
                            var keyProp = itemType.GetProperty("Key");
                            var valueProp = itemType.GetProperty("Value");
                            if (keyProp != null && valueProp != null)
                            {
                                var key = keyProp.GetValue(item) as string;
                                if (key != null)
                                {
                                    var dictValue = valueProp.GetValue(item);
                                    sanitized[key] = SanitizeValue(dictValue);
                                }
                            }
                        }
                        return sanitized;
                    }
                }
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

            // Preserve JsonArray type for Delivery string truncation
            if (value is JsonArray jsonArray)
            {
                var sanitized = new JsonArray();
                for (int i = 0; i < jsonArray.Count; i++)
                {
                    sanitized.Add(SanitizeValue(jsonArray[i]));
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
    }
}
