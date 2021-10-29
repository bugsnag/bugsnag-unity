using System;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public interface IEvent: IMetadataEditor, IUserEditor
    {
        string Context { get; set; }
        IAppWithState App { get; }
        IDeviceWithState Device { get; }
    }
}
