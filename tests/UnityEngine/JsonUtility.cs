using System;
namespace UnityEngine
{
    public class JsonUtility
    {
        public JsonUtility()
        {
        }
        public static T FromJson<T>(string json) {
            var t = new object();
            return (T)t;
        }
        public static string ToJson(object obj) {
            return string.Empty;
        }


    }
}
