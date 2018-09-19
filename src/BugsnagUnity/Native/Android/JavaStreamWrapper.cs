using System;
using System.IO;
using UnityEngine;

namespace BugsnagUnity
{
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
