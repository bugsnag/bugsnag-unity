using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity
{
    internal class NativePayloadClassWrapper
    {

        public AndroidJavaObject NativePointer;

        public NativePayloadClassWrapper(AndroidJavaObject nativePointer)
        {
            NativePointer = nativePointer;
        }

        public string GetNativeString(string key)
        {
            return NativePointer.Call<string>(key);
        }

        public void SetNativeString(string key, string value)
        {
            NativePointer.Call(key, value);
        }

        public int? GetNativeInt(string key)
        {
            var number = NativePointer.Call<AndroidJavaObject>(key);
            if (number == null)
            {
                return null;
            }
            return number.Call<int>("intValue");
        }

        public void SetNativeInt(string key, int? value)
        {
            if (value == null)
            {
                NativePointer.Call(key, null);
            }
            else
            {
                var javaInteger = new AndroidJavaObject("java.lang.Integer", (int)value);
                NativePointer.Call(key, javaInteger);
            }            
        }

        public long? GetNativeLong(string key)
        {
            var number = NativePointer.Call<AndroidJavaObject>(key);
            if (number == null)
            {
                return null;
            }
            return number.Call<long>("longValue");
        }

        public void SetNativeLong(string key, long? value)
        {
            if (value == null)
            {
                NativePointer.Call(key, null);
            }
            else
            {
                var javaInteger = new AndroidJavaObject("java.lang.Long", (long)value);
                NativePointer.Call(key, javaInteger);
            }
        }

        public double? GetNativeDouble(string key)
        {
            var number = NativePointer.Call<AndroidJavaObject>(key);
            if (number == null)
            {
                return null;
            }
            return number.Call<double>("doubleValue");
        }

        public void SetNativeDouble(string key, double? value)
        {
            if (value == null)
            {
                NativePointer.Call(key, null);
            }
            else
            {
                var javaInteger = new AndroidJavaObject("java.lang.Double", (double)value);
                NativePointer.Call(key, javaInteger);
            }
        }

        public TimeSpan? GetNativeTimespan(string key)
        {
            var millis = GetNativeDouble(key);
            if (millis == null)
            {
                return null;
            }
            return TimeSpan.FromMilliseconds((double)millis);
        }

        public void SetNativeTimespan(string key, TimeSpan? value)
        {
            if (value == null)
            {
                NativePointer.Call(key, null);
            }
            else
            {
                SetNativeDouble(key,value.Value.TotalMilliseconds);
            }
        }

        public bool? GetNativeBool(string key)
        {
            var value = NativePointer.Call<AndroidJavaObject>(key);
            if (value == null)
            {
                return null;
            }
            else
            {
                return value.Call<bool>("booleanValue");
            }
        }

        public void SetNativeBool(string key, bool? value)
        {
            if (value == null)
            {
                NativePointer.Call(key, null);
            }
            else
            {
                var javaBoolean = new AndroidJavaObject("java.lang.Boolean", (bool)value);
                NativePointer.Call(key, javaBoolean);
            }
        }

        public string[] GetNativeStringArray(string key)
        {
            var array = NativePointer.Call<AndroidJavaObject>(key);
            if (array == null)
            {
                return null;
            }
            var arraysClass = new AndroidJavaClass("java.util.Arrays");

            var list = arraysClass.CallStatic<AndroidJavaObject>("asList",array);

            var iterator = list.Call<AndroidJavaObject>("iterator");

            var returnList = new List<string>();

            while (iterator.Call<bool>("hasNext"))
            {
                var next = iterator.Call<string>("next");
                returnList.Add(next);
            }

            return returnList.ToArray();

        }

        public void SetNativeStringArray(string key, string[] value)
        {

            if (value == null)
            {
                NativePointer.Call(key, null);
                return;
            }

            AndroidJavaClass arrayClass = new AndroidJavaClass("java.lang.reflect.Array");

            AndroidJavaObject arrayObject = arrayClass.CallStatic<AndroidJavaObject>("newInstance", new AndroidJavaClass("java.lang.String"), value.Length);

            for (int i = 0; i < value.Length; ++i)
            {
                arrayClass.CallStatic("set", arrayObject, i, new AndroidJavaObject("java.lang.String", value[i]));
            }
            NativePointer.Call(key, arrayObject);

        }

        private Dictionary<string, object> GetDictFromNativeMap(AndroidJavaObject map)
        {
            var size = map.Call<int>("size");
            if (size > 0)
            {
                var keys = map.Call<AndroidJavaObject>("keySet");
                var iterator = keys.Call<AndroidJavaObject>("iterator");
                var dict = new Dictionary<string, object>();
                while (iterator.Call<bool>("hasNext"))
                {
                    var next = iterator.Call<AndroidJavaObject>("next");
                    var theKey = next.Call<string>("toString");
                    var theValue = map.Call<AndroidJavaObject>("get", theKey);
                    if (theValue != null)
                    {
                        var theValueString = theValue.Call<string>("toString");
                        dict.Add(theKey, theValueString);
                    }
                    else
                    {
                        dict.Add(theKey, null);
                    }
                }
                return dict;
            }
            return null;
        }

        public Dictionary<string, object> GetNativeDictionary(string key)
        {
            var map = NativePointer.Call<AndroidJavaObject>(key);
            if (map != null)
            {
                return GetDictFromNativeMap(map);
            }
            return null;          
        }

        public Dictionary<string, object> GetNativeMetadataSection(string key, string section)
        {
            var map = NativePointer.Call<AndroidJavaObject>(key,section);
            if (map != null)
            {
                return GetDictFromNativeMap(map);
            }
            return null;
        }

        public void SetNativeDictionary(string key,IDictionary<string, object> dict)
        {
            using (var map = NativeInterface.BuildJavaMapDisposable(dict))
            {
                NativePointer.Call(key,map);
            }
        }

        public void SetNativeMetadataSection(string key, string section, IDictionary<string, object> dict)
        {
            using (var map = NativeInterface.BuildJavaMapDisposable(dict))
            {
                NativePointer.Call(key, section, map);
            }
        }

        public DateTime? GetNativeDateTime(string key)
        {
            var nativeDate = NativePointer.Call<AndroidJavaObject>(key);

            if (nativeDate == null)
            {
                return null;
            }

            var timeStamp = nativeDate.Call<long>("getTime");

            var date = (new DateTime(1970, 1, 1)).AddMilliseconds(timeStamp);

            return date;
        }

        public void SetNativeDateTime(string key, DateTime? dateTime)
        {
            if (dateTime == null)
            {
                NativePointer.Call(key, null);
            }
            else
            {
                var mill = (dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Value.TotalMilliseconds;
                var javaDate = new AndroidJavaObject("java.util.Date", (long)mill);
                NativePointer.Call(key,javaDate);
            }
        }

    }
}
