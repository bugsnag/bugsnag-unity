using System;
using System.Collections.Generic;

namespace BugsnagUnity.Payload
{
    public interface IThread
    {
        string Id { get; set; }
        bool? ErrorReportingThread { get; }
        string Name { get; set; }
        List<IStackframe> Stacktrace { get; }
        string Type { get; }
    }
}
