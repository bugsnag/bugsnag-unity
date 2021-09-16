using System;
namespace UnityEngine
{
    public class Screen
    {
        public static float dpi { get; }

        public static Resolution currentResolution { get; } = new Resolution();
    }
}
