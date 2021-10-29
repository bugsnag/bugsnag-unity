using System;
using System.Collections.Generic;

namespace BugsnagUnity.Payload
{
    public interface IBreadcrumb
    {
        string Message { get; set; }

        Dictionary<string, object> Metadata { get; set; }

        DateTime? Timestamp { get; }

        string Type { get; set; }
    }
}
