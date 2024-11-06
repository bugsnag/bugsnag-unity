#if ((UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR) || BGS_COCOA_DEV
using System;
namespace BugsnagUnity
{
    public class NativePayloadClassWrapper
    {
        internal IntPtr NativePointer;

        public NativePayloadClassWrapper(IntPtr nativePointer)
        {
            NativePointer = nativePointer;
        }

        internal string GetNativeString(string key)
        {
            return NativeCode.bugsnag_getValueAsString(NativePointer, key);
        }

        internal void SetNativeString(string key, string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }
            NativeCode.bugsnag_setStringValue(NativePointer, key, value);
        }

        internal bool? GetNativeBool(string key)
        {
            var result = NativeCode.bugsnag_getValueAsString(NativePointer, key).ToLower();
            if (result == null)
            {
                return null;
            }
            return result == "1" || result == "true";
        }

        internal void SetNativeBool(string name, bool? value)
        {
            var stringValue = value == null ? "null" : value.ToString();
            NativeCode.bugsnag_setBoolValue(NativePointer, name, stringValue);
        }

        internal double? GetNativeDouble(string key)
        {
            var value = NativeCode.bugsnag_getValueAsString(NativePointer, key);

            if (value == null)
            {
                return null;
            }
            else
            {
                return double.Parse(value);
            }
        }

        internal void SetNativeDouble(string key, double? value)
        {
            NativeCode.bugsnag_setNumberValue(NativePointer,key,value == null ? "null" : value.ToString());
        }

        internal long? GetNativeLong(string key)
        {
            var value = NativeCode.bugsnag_getValueAsString(NativePointer, key);

            if (value == null)
            {
                return null;
            }
            else
            {
                return long.Parse(value);
            }
        }

        internal void SetNativeLong(string key, long? value)
        {
            NativeCode.bugsnag_setNumberValue(NativePointer, key, value == null ? "null" : value.ToString());
        }

        internal DateTimeOffset? GetNativeDate(string key)
        {
            var timeStamp = NativeCode.bugsnag_getTimestampFromDateInObject(NativePointer, key);
            if (timeStamp < 0)
            {
                return null;
            }
            else
            {
                DateTimeOffset dateTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
                dateTime = dateTime.AddSeconds(timeStamp);
                return dateTime;
            }
        }

        internal void SetNativeDate(DateTimeOffset? startedAt,string key)
        {
            if (startedAt == null)
            {
                NativeCode.bugsnag_setTimestampFromDateInObject(NativePointer, key, -1);
            }
            else
            {
                var dateTime = startedAt.Value - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
                NativeCode.bugsnag_setTimestampFromDateInObject(NativePointer, key, dateTime.TotalSeconds);
            }
        }
    }
}
#endif