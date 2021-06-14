using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BugsnagUnity.Payload
{
    public interface IPayload
    {
        Uri Endpoint { get; }

        KeyValuePair<string, string>[] Headers { get; }
    }
}
