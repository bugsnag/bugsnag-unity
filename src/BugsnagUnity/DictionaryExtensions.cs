using System;
using System.Collections.Generic;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
  static class DictionaryExtensions
  {
    private static IntPtr Arrays { get; } = AndroidJNI.FindClass("java/util/Arrays");

    private static IntPtr ToStringMethod { get; } = AndroidJNIHelper.GetMethodID(Arrays, "toString", "([Ljava/lang/Object;)Ljava/lang/String;", true);

    internal static void PopulateDictionaryFromAndroidData(this IDictionary<string, object> dictionary, AndroidJavaObject source)
    {
      using (var set = source.Call<AndroidJavaObject>("entrySet"))
      using (var iterator = set.Call<AndroidJavaObject>("iterator"))
      {
        while (iterator.Call<bool>("hasNext"))
        {
          using (var mapEntry = iterator.Call<AndroidJavaObject>("next"))
          {
            var key = mapEntry.Call<string>("getKey");
            using (var value = mapEntry.Call<AndroidJavaObject>("getValue"))
            {
              if (value != null)
              {
                using (var @class = value.Call<AndroidJavaObject>("getClass"))
                {
                  if (@class.Call<bool>("isArray"))
                  {
                    var args = AndroidJNIHelper.CreateJNIArgArray(new[] {value});
                    var formattedValue = AndroidJNI.CallStaticStringMethod(Arrays, ToStringMethod, args);
                    dictionary.AddToPayload(key, formattedValue);
                  }
                  else
                  {
                    dictionary.AddToPayload(key, value.Call<string>("toString"));
                  }
                }
              }
            }
          }
        }
      }
    }
  }
}
