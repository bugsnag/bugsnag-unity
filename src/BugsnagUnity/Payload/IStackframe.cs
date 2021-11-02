using System;
namespace BugsnagUnity.Payload
{
    public interface IStackframe
    {
        string FrameAddress { get; set; }

        string IsLr { get; set; }

        string IsPc { get; set; }

        string MachoFile { get; set; }

        string MachoLoadAddress { get; set; }

        string MachoUuid { get; set; }

        string MachoVmAddress { get; set; }

        string Method { get; set; }

        string SymbolAddress { get; set; }

        string File { get; set; }

        bool InProject { get; set; }

        int LineNumber { get; set; }
    }
}
