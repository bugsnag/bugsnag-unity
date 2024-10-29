using System;
using System.Linq;
using System.Reflection;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    internal class PerformanceHelper
    {
        const string BUGSNAG_PERFORMANCE_ASSEMBLY_NAME = "BugsnagUnityPerformance.BugsnagPerformance";
        const string GET_PERFORMANCE_STATE_METHOD_NAME = "GetPerformanceState";

        static MethodInfo _getPerformanceStateMethodInfo;
        private static bool _isPerformanceLibraryAvailable = true;

        [Serializable]
        private class PerformanceState
        {
            public string currentContextSpanId;
            public string currentContextTraceId;
            public PerformanceState(string currentContextSpanId, string currentContextTraceId)
            {
                this.currentContextSpanId = currentContextSpanId;
                this.currentContextTraceId = currentContextTraceId;
            }
        }

        internal static Correlation GetCurrentPerformanceSpanContext()
        {
            if (!_isPerformanceLibraryAvailable)
            {
                return null;
            }

            try
            {
                InitializeGetCurrentContextInternalMethodIfNeeded();

                string result = (string)_getPerformanceStateMethodInfo?.Invoke(null, null);

                if (string.IsNullOrEmpty(result))
                {
                    return null;
                }

                var performanceState = JsonUtility.FromJson<PerformanceState>(result);

                if (performanceState == null)
                {
                    return null;
                }

                var correlation = new Correlation(performanceState.currentContextTraceId, performanceState.currentContextSpanId);

                return correlation;
            }
            catch (Exception ex)
            {
                // Silently handle case where performance library is not available or out of date
                _isPerformanceLibraryAvailable = false;
                return null;
            }
        }

        private static void InitializeGetCurrentContextInternalMethodIfNeeded()
        {
            if (_getPerformanceStateMethodInfo != null) return;

            var bugsnagPerformanceType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(BUGSNAG_PERFORMANCE_ASSEMBLY_NAME))
                .FirstOrDefault(type => type != null);

            _getPerformanceStateMethodInfo = bugsnagPerformanceType?.GetMethod(
                GET_PERFORMANCE_STATE_METHOD_NAME,
                BindingFlags.NonPublic | BindingFlags.Static
            );
        }

    }
}
