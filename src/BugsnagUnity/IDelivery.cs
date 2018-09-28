using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using BugsnagUnity.Payload;
using UnityEngine;
using UnityEngine.Networking;

namespace BugsnagUnity
{
  interface IDelivery
  {
    void Send(IPayload payload);
  }

  class Delivery : IDelivery
  {
    Thread Worker { get; }

    BlockingQueue<IPayload> Queue { get; }

    GameObject DispatcherObject { get; }

    internal Delivery()
    {
      DispatcherObject = new GameObject("Bugsnag thread dispatcher");
      DispatcherObject.AddComponent<MainThreadDispatchBehaviour>();

      Queue = new BlockingQueue<IPayload>();
      Worker = new Thread(ProcessQueue) { IsBackground = true };
      Worker.Start();
    }

    void ProcessQueue()
    {
      while (true)
      {
        try
        {
          var payload = Queue.Dequeue();
          using (var stream = new MemoryStream())
          using (var reader = new StreamReader(stream))
          using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
          {
            SimpleJson.SimpleJson.SerializeObject(payload, writer);
            writer.Flush();
            stream.Position = 0;
            var body = Encoding.UTF8.GetBytes(reader.ReadToEnd());
            MainThreadDispatchBehaviour.Instance().Enqueue(PushToServer(payload, body));
          }
        }
        catch (System.Exception)
        {
          // ensure that the thread carries on processing error reports
        }
      }
    }

    public void Send(IPayload payload)
    {
      Queue.Enqueue(payload);
    }

    IEnumerator PushToServer(IPayload payload, byte[] body)
    {
      using (var req = new UnityWebRequest(payload.Endpoint.ToString()))
      {
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Bugsnag-Sent-At", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        foreach (var header in payload.Headers)
        {
          req.SetRequestHeader(header.Key, header.Value);
        }
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.method = UnityWebRequest.kHttpVerbPOST;

        // we are using the deprecated Send method here so that we can continue
        // to support unity 5.6, once this support is dropped we can use the
        // newer SendWebRequest method
        yield return req.Send();
        while (!req.isDone)
          yield return new WaitForEndOfFrame();

        if (req.responseCode >= 200 && req.responseCode < 300)
        {
          // success!
        }
        // once we can drop support for unity 5.6 we can use req.isNetworkError
        // instead of req.error != null. According to the unity docs though this
        // should have the same effect
        else if (req.responseCode >= 500 || req.error != null)
        {
          // something is wrong with the server/connection, should retry
          Send(payload);
        }
        else if (req.error != null)
        {
          Debug.LogWarning("Bugsnag: " + req.error);
        }
      }
    }
  }
}
