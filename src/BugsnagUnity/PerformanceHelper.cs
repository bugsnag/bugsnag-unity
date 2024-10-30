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

        internal static Correlation GetCurrentPerformanceSpanContext()
        {
            if (!_isPerformanceLibraryAvailable)
            {
                return null;
            }

            try
            {
                InitializeGetCurrentContextInternalMethodIfNeeded();

                byte[] data = (byte[])_getPerformanceStateMethodInfo?.Invoke(null, null);

                if (data == null)
                {
                    return null;
                }
                
                int spanIdLength = BitConverter.ToInt32(data, 0);
                string spanId = System.Text.Encoding.UTF8.GetString(data, 4, spanIdLength);

                int traceIdLength = BitConverter.ToInt32(data, 4 + spanIdLength);
                string traceId = System.Text.Encoding.UTF8.GetString(data, 4 + spanIdLength + 4, traceIdLength);


                var correlation = new Correlation(spanId, traceId);

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
