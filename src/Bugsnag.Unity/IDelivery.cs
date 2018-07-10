using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Bugsnag.Unity.Payload;
using UnityEngine;

namespace Bugsnag.Unity
{
  interface IDelivery
  {
    void Send(IPayload payload);
  }

  class Delivery : IDelivery
  {
    Thread Worker { get; }

    BlockingQueue<IPayload> Queue { get; }

    internal Delivery()
    {
      Queue = new BlockingQueue<IPayload>();
      Worker = new Thread(ProcessQueue) { IsBackground = true };
      Worker.Start();
    }

    void ProcessQueue()
    {
      while (true)
      {
        var payload = Queue.Dequeue();

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

    public void Send(IPayload payload)
    {
      Queue.Enqueue(payload);
    }
  }

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
            connection.Call("addRequestProperty", "Content-Type", "application/json; charset=utf-8");
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

  class JavaStreamWrapper : Stream
  {
    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotImplementedException();

    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    AndroidJavaObject JavaStream { get; }

    bool Disposed { get; set; }

    public JavaStreamWrapper(AndroidJavaObject javaStream)
    {
      JavaStream = javaStream;
    }

    public override void Flush()
    {
      JavaStream.Call("flush");
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
      throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      JavaStream.Call("write", buffer, offset, count);
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!Disposed)
      {
        try
        {
          JavaStream.Call("close");
        }
        catch (System.Exception)
        {
          // match the bugsnag-android behaviour
        }
        finally
        {
          Disposed = true;
        }
      }
    }

    ~JavaStreamWrapper()
    {
      Dispose(false);
    }
  }

  class BlockingQueue<T>
  {
    Queue<T> Queue { get; }
    object QueueLock { get; }

    internal BlockingQueue()
    {
      QueueLock = new object();
      Queue = new Queue<T>();
    }

    internal void Enqueue(T item)
    {
      lock (QueueLock)
      {
        Queue.Enqueue(item);
        Monitor.Pulse(QueueLock);
      }
    }

    internal T Dequeue()
    {
      lock (QueueLock)
      {
        while (Queue.Count == 0)
        {
          Monitor.Wait(QueueLock);
        }

        return Queue.Dequeue();
      }
    }
  }
}
