using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity
{
    public class NativePayloadClassWrapper
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

        public bool? GetNativeBool(string key)
        {
            var value = NativePointer.Call<AndroidJavaObject>(key);
            if (value == null)
            {
                return null;
            }
            else
            {
                return NativePointer.Call<bool>(key);
            }
        }

        public void SetNativeBool(string key, bool? value)
        {
            NativePointer.Call(key, value);
        }

        public Dictionary<string, object> GetNativeDictionary(string key)
        {
            var map = NativePointer.Call<AndroidJavaObject>(key);

            if (map != null)
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
                        var theValue = map.Call<AndroidJavaObject>("get",theKey);
                        var theValueString = theValue.Call<string>("toString");
                        dict.Add(theKey,theValueString);
                    }
                    return dict;
                }
                return null;
            }
            return null;          
        }


      


    }
}
