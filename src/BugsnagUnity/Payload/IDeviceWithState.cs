using System;

namespace BugsnagUnity.Payload
{
    public interface IDeviceWithState : IDevice
    {
        ulong? FreeDisk { get; set; }
        ulong? FreeMemory { get; set; }
        string Orientation { get; set; }
        DateTime? Time { get; set; }
    }
}