using System;
namespace BugsnagUnity.Payload
{
    public interface IThread
    {
        long? Id { get; set; }
        bool? ErrorReportingThread { get; }
        string Name { get; set; }
    }
}
