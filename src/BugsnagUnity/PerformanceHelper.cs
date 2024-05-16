#define PERFORMANCE_HELPER_LOG
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
            Log("Entering GetCurrentPerformanceSpanContext");

            if (!_isPerformanceLibraryAvailable)
            {
                Log("Performance library is not available");
                return null;
            }

            try
            {
                Log("Initializing GetCurrentContextInternalMethod if needed");
                InitializeGetCurrentContextInternalMethodIfNeeded();

                Log("Invoking _getCurrentContextInternalMethod");
                string result = (string)_getPerformanceStateMethodInfo?.Invoke(null, null);

                Log("Result from _getCurrentContextInternalMethod: " + result);

                if (string.IsNullOrEmpty(result))
                {
                    Log("Result is null or empty");
                    return null;
                }

                Log("Parsing result to CorrelationTransfer");
                var performanceState = JsonUtility.FromJson<PerformanceState>(result);

                if (performanceState == null)
                {
                    Log("Deserialization failed, CorrelationTransfer is null");
                    return null;
                }

                Log("Performance library context: traceId=" + performanceState.currentContextTraceId + ", spanId=" + performanceState.currentContextSpanId);

                var correlation = new Correlation(performanceState.currentContextTraceId, performanceState.currentContextSpanId);

                Log("Performance library correlation: spanId=" + correlation.SpanId + ", traceId=" + correlation.TraceId);

                Log("Exiting GetCurrentPerformanceSpanContext with correlation");
                return correlation;
            }
            catch (Exception ex)
            {
                Log("Exception caught: " + ex.Message);
                // Silently handle case where performance library is not available or out of date
                Log("Performance library is not available or out of date");
                _isPerformanceLibraryAvailable = false;

                Log("Exiting GetCurrentPerformanceSpanContext with null due to exception");
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

        private static void Log(string message)
        {
            #if PERFORMANCE_HELPER_LOG
                UnityEngine.Debug.Log(message);
            #endif
        }
    }
}
