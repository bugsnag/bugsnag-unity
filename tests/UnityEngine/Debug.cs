﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine
{
    public class Debug
    {
        public static bool isDebugBuild { get; }
        public static void LogError(object message) { }
        public static void LogWarning(object message) { }

    }
}
