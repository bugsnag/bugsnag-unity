using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
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
    internal AndroidDelivery()
    {
      // ensure that the class loader has loaded some sort of java object or
      // we will not be able to create any java objects on another thread
      new AndroidJavaObject("java.lang.Object");
    }

    public void Send(IPayload payload)
    {
      // we need to ensure that the current thread is attached to the JVM
      // this should be a no-op if it already is
      AndroidJNI.AttachCurrentThread();

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
}
