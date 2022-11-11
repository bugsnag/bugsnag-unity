using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BugsnagUnity.Payload
{
    public interface IPayload : ICloneable
    {
        Uri Endpoint { get; set; }

        KeyValuePair<string, string>[] Headers { get; set; }

        string Id { get; set; }

        PayloadType PayloadType { get; }

        Dictionary<string, object> GetSerialisablePayload();
    }

    public enum PayloadType
    {
        Session,
        Event
    }
}
