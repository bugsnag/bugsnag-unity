using System;
using System.Collections.Generic;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public interface IEvent: IMetadataEditor, IUserEditor
    {
        string ApiKey { get; set; }

        IAppWithState App { get; }

        string Context { get; set; }

        IDeviceWithState Device { get; }

        List<IBreadcrumb> Breadcrumbs { get; }

        List<IError> Errors { get; }

        string GroupingHash { get; set; }

        string Severity { get; }

        List<IThread> Threads { get; }

        bool Unhandled { get; set; }
    }
}
