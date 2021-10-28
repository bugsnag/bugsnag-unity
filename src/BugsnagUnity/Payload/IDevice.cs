using System.Collections.Generic;

namespace BugsnagUnity
{
    public interface IDevice
    {
        string BrowserName { get; set; }
        string BrowserVersion { get; set; }
        string[] CpuAbi { get; set; }
        string Id { get; set; }
        bool? Jailbroken { get; set; }
        string Locale { get; set; }
        string Manufacturer { get; set; }
        string Model { get; set; }
        string ModelNumber { get; set; }
        string OsName { get; set; }
        string OsVersion { get; set; }
        Dictionary<string, object> RuntimeVersions { get; set; }
        int? TotalMemory { get; set; }
        string UserAgent { get; set; }
    }
}