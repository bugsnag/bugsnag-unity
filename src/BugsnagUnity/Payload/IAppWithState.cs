using System;

namespace BugsnagUnity.Payload
{
    public interface IAppWithState : IApp
    {
        TimeSpan? Duration { get; set; }
        TimeSpan? DurationInForeground { get; set; }
        bool? InForeground { get; set; }
        bool? IsLaunching { get; set; }
    }
}