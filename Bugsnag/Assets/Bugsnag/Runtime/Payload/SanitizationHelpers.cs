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
    }
}
