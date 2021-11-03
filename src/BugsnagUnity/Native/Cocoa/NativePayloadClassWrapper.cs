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
            if (value == null)
            {
                value = string.Empty;
            }
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

        internal double GetNativeDouble(string key)
        {
            return NativeCode.bugsnag_getDoubleValue(NativePointer, key);
        }

        internal void SetNativeDouble(string key, double value)
        {
            NativeCode.bugsnag_setDoubleValue(NativePointer,key,value);
        }

        internal long? GetNativeLong(string key)
        {
            long value = NativeCode.bugsnag_getLongValue(NativePointer, key);
            if (value < 0)
            {
                return null;
            }
            else
            {
                return value;
            }
        }

        internal void SetNativeLong(string key, long? value)
        {

            if (value == null)
            {
                NativeCode.bugsnag_setLongValue(NativePointer, key, -1);
            }
            else
            {
                NativeCode.bugsnag_setLongValue(NativePointer,key, (long)value);
            }
            
        }

        internal DateTime? GetNativeDate(string key)
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

        internal void SetNativeDate(DateTime? startedAt,string key)
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
