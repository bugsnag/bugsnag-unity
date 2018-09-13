using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace BugsnagUnity.Payload
{
  /// <summary>
  /// Represents a set of Bugsnag payload stacktrace lines that are generated from a single StackTrace provided
  /// by the runtime.
  /// </summary>
  class StackTrace : IEnumerable<StackTraceLine>
  {
    StackFrame[] StackFrames { get; }

    internal StackTrace(StackFrame[] stackFrames)
    {
      StackFrames = stackFrames;
    }

    public IEnumerator<StackTraceLine> GetEnumerator()
    {
      if (StackFrames == null)
      {
        yield break;
      }

      foreach (var frame in StackFrames)
      {
        yield return StackTraceLine.FromStackFrame(frame);
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }

  /// <summary>
  /// Represents an individual stack trace line in the Bugsnag payload.
  /// </summary>
  class StackTraceLine : Dictionary<string, object>
  {
    internal static StackTraceLine FromStackFrame(StackFrame stackFrame)
    {
      var method = stackFrame.GetMethod();
      var file = stackFrame.GetFileName();
      var lineNumber = stackFrame.GetFileLineNumber();
      var methodName = new Method(method).DisplayName();

      return new StackTraceLine(file, lineNumber, methodName);
    }

    internal StackTraceLine(string file, int? lineNumber, string methodName)
    {
      this.AddToPayload("file", file);
      if (lineNumber.HasValue)
      {
        this.AddToPayload("lineNumber", lineNumber.Value);
      }
      this.AddToPayload("method", methodName);
    }

    internal string FileName
    {
      get
      {
        return this.Get("file") as string;
      }
      set
      {
        this.AddToPayload("file", value);
      }
    }

    internal string MethodName
    {
      get
      {
        return this.Get("method") as string;
      }
      set
      {
        this.AddToPayload("method", value);
      }
    }
  }
}
