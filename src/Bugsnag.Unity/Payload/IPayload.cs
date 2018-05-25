using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bugsnag.Unity.Payload
{
  interface IPayload
  {
    Uri Endpoint { get; }

    KeyValuePair<string, string>[] Headers { get; }
  }
}
