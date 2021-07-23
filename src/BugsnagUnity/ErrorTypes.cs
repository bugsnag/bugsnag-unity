using System;
namespace BugsnagUnity
{
    public class ErrorTypes
    {

        public bool ANRs = true;
        public bool AppHangs = true;
        public bool CppExceptions = true;
        public bool MachExceptions = true;
        public bool NdkCrashes = true;
        public bool OOMs = true;
        public bool Signals = true;
        public bool UnhandledExceptions = true;
        public bool UnityLogLogs = true;
        public bool UnityWarningLogs = true;
        public bool UnityAssertLogs = true;
        public bool UnityErrorLogs = true;


        public void SetAllDisabled()
        {
            ANRs = false;
            AppHangs = false;
            CppExceptions = false;
            MachExceptions = false;
            NdkCrashes = false;
            OOMs = false;
            Signals = false;
            UnhandledExceptions = false;
            UnityLogLogs = false;
            UnityWarningLogs = false;
            UnityAssertLogs = false;
            UnityErrorLogs = false;
        }

    }
}
