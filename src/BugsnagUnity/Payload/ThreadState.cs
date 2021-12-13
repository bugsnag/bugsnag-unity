using System;
namespace BugsnagUnity.Android.Payload
{
    public enum ThreadState
    {
        NEW,
        BLOCKED,
        RUNNABLE,
        TERMINATED,
        TIMED_WAITING,
        WAITING,
        UNKNOWN
    }
}
