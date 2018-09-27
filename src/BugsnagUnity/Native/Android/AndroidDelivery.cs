using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
  class AndroidDelivery : IDelivery
  {
    Thread Worker { get; }

    BlockingQueue<IPayload> Queue { get; }

    internal AndroidDelivery()
    {
      // ensure that the class loader has loaded some sort of java object or
      // we will not be able to create any java objects on another thread
      new AndroidJavaObject("java.lang.Object");

      Queue = new BlockingQueue<IPayload>();
      Worker = new Thread(ProcessQueue) { IsBackground = true };
      Worker.Start();
    }

    void ProcessQueue()
    {
      // we need to ensure that the current thread is attached to the JVM
      // this should be a no-op if it already is
      AndroidJNI.AttachCurrentThread();

      while (true)
      {
        var payload = Queue.Dequeue();

        using (var url = new AndroidJavaObject("java.net.URL", payload.Endpoint.ToString()))
        using (var connection = url.Call<AndroidJavaObject>("openConnection"))
        {
          try
          {
            connection.Call("setDoOutput", true);
            connection.Call("setChunkedStreamingMode", 0);
            connection.Call("addRequestProperty", "Content-Type", "application/json");
            connection.Call("addRequestProperty", "Bugsnag-Sent-At", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));

            foreach (var header in payload.Headers)
            {
              connection.Call("addRequestProperty", header.Key, header.Value);
            }

            using (var outputStream = connection.Call<AndroidJavaObject>("getOutputStream"))
            using (var streamMapper = new JavaStreamWrapper(outputStream))
            using (var writer = new StreamWriter(streamMapper, new UTF8Encoding(false)) { AutoFlush = false })
            {
              SimpleJson.SimpleJson.SerializeObject(payload, writer);
              writer.Flush();
            }

            var code = connection.Call<int>("getResponseCode");
          }
          finally
          {
            connection.Call("disconnect");
          }
        }
      }
    }

    public void Send(IPayload payload)
    {
      Queue.Enqueue(payload);
    }
  }
}
