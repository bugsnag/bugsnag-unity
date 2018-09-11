using System;
using System.Collections.Generic;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
  static class DictionaryExtensions
  {
    internal static void PopulateDictionaryFromAndroidData(this IDictionary<string, object> dictionary, AndroidJavaObject source)
    {
      using (var set = source.Call<AndroidJavaObject>("entrySet"))
      using (var iterator = set.Call<AndroidJavaObject>("iterator"))
      {
        while (iterator.Call<bool>("hasNext"))
        {
          using (var mapEntry = iterator.Call<AndroidJavaObject>("next"))
          {
            var key = mapEntry.CallStringMethod("getKey");
            using (var value = mapEntry.Call<AndroidJavaObject>("getValue"))
            {
              if (value != null)
              {
                using (var @class = value.Call<AndroidJavaObject>("getClass"))
                {
                  if (@class.Call<bool>("isArray"))
                  {
                    var values = AndroidJNIHelper.ConvertFromJNIArray<string[]>(value.GetRawObject());
                    dictionary.AddToPayload(key, values);
                  }
                  else
                  {
                    dictionary.AddToPayload(key, value.CallStringMethod("toString"));
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
