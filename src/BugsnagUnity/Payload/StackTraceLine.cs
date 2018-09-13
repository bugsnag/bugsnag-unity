using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace BugsnagUnity.Payload
{
  /// <summary>
  /// Represents a set of Bugsnag payload stacktrace lines that are generated from a single StackTrace provided
  /// by the runtime.
  /// </summary>
  class StackTrace : IEnumerable<StackTraceLine>
  {
    string OriginalStackTrace { get; }

    StackFrame[] StackFrames { get; }

    private static string[] StringSplit { get; } = { "\n", "\r\n" };

    /// <summary>
    /// Looks for lines that have matching parentheses. This indicates that
    /// the line contains a method call.
    /// </summary>
    private static Regex StackTraceLineRegex { get; } = new Regex(@"(?<method>\S+\s*\(.*?\))\s(?:(?:\[.*\]\s*in\s|\(at\s*\s*)(?<file>.*):(?<linenumber>\d+))?");

    internal StackTrace(string stackTrace)
    {
      OriginalStackTrace = stackTrace;
    }

    internal StackTrace(StackFrame[] stackFrames)
    {
      StackFrames = stackFrames;
    }

    public IEnumerator<StackTraceLine> GetEnumerator()
    {
      if (OriginalStackTrace != null)
      {
        foreach (var item in OriginalStackTrace.Split(StringSplit, StringSplitOptions.RemoveEmptyEntries))
        {
          var match = StackTraceLineRegex.Match(item);

          if (match.Success)
          {
            var method = match.Groups["method"].Success ? match.Groups["method"].Value : null;
            var file = match.Groups["file"].Success ? match.Groups["file"].Value : null;
            var line = match.Groups["linenumber"].Success ? match.Groups["linenumber"].Value : null;

            if (int.TryParse(line, out var lineNumber))
            {
              yield return new StackTraceLine(file, lineNumber, method);
            }
            else
            {
              yield return new StackTraceLine(file, null, method);
            }
          }
          else
          {
            yield return new StackTraceLine(null, null, item);
          }
        }
      }

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
