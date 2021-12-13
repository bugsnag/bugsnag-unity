using System;
using System.Collections.Generic;

namespace BugsnagUnity.Payload
{
    public interface IBreadcrumb
    {
        string Message { get; set; }

        IDictionary<string, object> Metadata { get; set; }

        DateTime? Timestamp { get; }

        BreadcrumbType Type { get; set; }
    }
}
