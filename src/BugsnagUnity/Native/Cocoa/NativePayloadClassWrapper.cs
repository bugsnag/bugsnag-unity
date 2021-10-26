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
            return NativeCode.bugsnag_getStringValue(NativePointer, key);
        }

        internal void SetNativeString(string key, string value)
        {
            NativeCode.bugsnag_setStringValue(NativePointer, key, value);
        }

        internal bool? GetNativeBool(string key)
        {
            var result = NativeCode.bugsnag_getBoolValue(NativePointer, key);
            return result == "null" ? null : (bool?)bool.Parse(result);
        }

        internal void SetNativeBool(string name, bool? value)
        {
            var stringValue = value == null ? "null" : value.ToString();
            NativeCode.bugsnag_setBoolValue(NativePointer, name, stringValue);
        }

        internal DateTime? GetNativeTimestamp(string key)
        {
            var timeStamp = NativeCode.bugsnag_getTimestampFromDateInObject(NativePointer, key);
            if (timeStamp < 0)
            {
                return null;
            }
            else
            {
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dateTime = dateTime.AddSeconds(timeStamp);
                return dateTime;
            }
        }

        internal void SetNativeTimeStamp(DateTime? startedAt,string key)
        {
            if (startedAt == null)
            {
                NativeCode.bugsnag_setTimestampFromDateInObject(NativePointer, key, -1);
            }
            else
            {
                var dateTime = startedAt.Value - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                NativeCode.bugsnag_setTimestampFromDateInObject(NativePointer, key, dateTime.TotalSeconds);
            }
        }
    }
}
