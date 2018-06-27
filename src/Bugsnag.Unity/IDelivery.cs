using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Bugsnag.Unity.Payload;

namespace Bugsnag.Unity
{
  interface IDelivery
  {
    void Send(IPayload payload);
  }

  class Delivery : IDelivery
  {
    public void Send(IPayload payload)
    {
      using (var client = new WebClient())
      {
        client.Headers.Add("Content-Type", "application/json; charset=utf-8");
        client.Headers.Add("Bugsnag-Sent-At", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        foreach (var header in payload.Headers)
        {
          client.Headers.Add(header.Key, header.Value);
        }

        using (var stream = client.OpenWrite(payload.Endpoint))
        using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
        {
          SimpleJson.SimpleJson.SerializeObject(payload, writer);
          writer.Flush();
        }
      }
    }
  }

  class AndroidDelivery : IDelivery
  {
    public void Send(IPayload payload)
    {
      throw new NotImplementedException();
    }
  }
}
