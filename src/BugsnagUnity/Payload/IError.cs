using System;
using System.Collections.Generic;

namespace BugsnagUnity.Payload
{
    public interface IError
    {
        string ErrorClass { get; set; }

        string ErrorMessage { get; set; }

        List<IStackframe> Stacktrace { get; }
    }
}
