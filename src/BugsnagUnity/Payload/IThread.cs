using System;
namespace BugsnagUnity.Payload
{
    public interface IThread
    {
        long Id { get; }
        bool ErrorReportingThread { get; }
        string Name { get; }
    }
}
