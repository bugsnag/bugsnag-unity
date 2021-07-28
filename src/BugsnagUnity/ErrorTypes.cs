using System;
namespace BugsnagUnity
{
    [Serializable]
    [Flags]
    public enum ErrorTypes
    {
       
        ANRs = 0,

        AppHangs = 1,

        OOMs = 2,

        NativeCrashes = 3,

        UnhandledExceptions = 4,

        UnityLogLogs = 5,

        UnityWarningLogs = 6,

        UnityAssertLogs = 7,

        UnityErrorLogs = 8
    }
}
