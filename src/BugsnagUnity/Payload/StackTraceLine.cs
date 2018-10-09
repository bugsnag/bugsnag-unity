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
    private StackTraceLine[] StackTraceLines { get; }

    internal StackTrace(StackFrame[] stackFrames)
    {
      StackTraceLines = new StackTraceLine[stackFrames.Length];
      for (int i = 0; i < stackFrames.Length; i++)
      {
        StackTraceLines[i] = StackTraceLine.FromStackFrame(stackFrames[i]);
      }
    }

    internal StackTrace(string stackTrace)
    {
      string[] lines = stackTrace.Split(new[] { System.Environment.NewLine },
                                        System.StringSplitOptions.RemoveEmptyEntries);
      StackTraceLines = new StackTraceLine[lines.Length];
      for (int i = 0; i < lines.Length; i++)
      {
        StackTraceLines[i] = StackTraceLine.FromLogMessage(lines[i]);
      }
    }

    public IEnumerator<StackTraceLine> GetEnumerator()
    {
      foreach (var frame in StackTraceLines)
      {
        yield return frame;
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
  public class StackTraceLine : Dictionary<string, object>
  {
    private static Regex StackTraceLineRegex { get; } = new Regex(@"(?<method>[^()]+\(.*?\))\s(?:(?:\[.*\]\s*in\s|\(at\s*\s*)(?<file>.*):(?<linenumber>\d+))?");

    public static StackTraceLine FromLogMessage(string message) {
      Match match = StackTraceLineRegex.Match(message);
      if (match.Success)
      {
        int? lineNumber = null;
        int parsedValue;
        if (System.Int32.TryParse(match.Groups["linenumber"].Value, out parsedValue))
        {
          lineNumber = parsedValue;
        }
        return new StackTraceLine(match.Groups["file"].Value,
                                  lineNumber,
                                  match.Groups["method"].Value);
      }
      return new StackTraceLine("", null, message);
    }

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

    public string File
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

    public int? LineNumber
    {
      get
      {
        return this.Get("lineNumber") as int?;
      }
      set
      {
        this.AddToPayload("lineNumber", value);
      }
    }

    public string Method
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
