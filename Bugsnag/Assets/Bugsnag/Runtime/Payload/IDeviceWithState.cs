﻿using System;

namespace BugsnagUnity.Payload
{
    public interface IDeviceWithState : IDevice
    {
        long? FreeDisk { get; set; }
        long? FreeMemory { get; set; }
        string Orientation { get; set; }
        DateTimeOffset? Time { get; set; }
    }
}