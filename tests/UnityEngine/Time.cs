using System;
namespace UnityEngine
{
    public class Time
    {
        public static float realtimeSinceStartup
        {
            get {
                var now = DateTime.UtcNow;
                var startOfDay = new DateTime(now.Year, now.Month, now.Day,0,0,0);
                return (float)(now - startOfDay).TotalSeconds;
            }
            set { }
        }
    }
}
