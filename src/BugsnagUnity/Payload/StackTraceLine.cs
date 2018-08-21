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
    System.Exception OriginalException { get; }

    string OriginalStackTrace { get; }

    StackFrame[] AlternativeStackTrace { get; }

    private static string[] StringSplit { get; } = { Environment.NewLine };

    /// <summary>
    /// Looks for lines that have matching parentheses. This indicates that
    /// the line contains a method call.
    /// </summary>
    private static Regex StackTraceLineRegex { get; } = new Regex(@"(?<method>\S+\s*\(.*?\))\s*(?:(?:\[.*\]\s*in\s|\(at\s*\s*)(?<file>.*):(?<linenumber>\d+))?");

    internal StackTrace(string stackTrace)
    {
      OriginalStackTrace = stackTrace;
    }

    internal StackTrace(System.Exception exception, StackFrame[] alternativeStackTrace)
    {
      OriginalException = exception;
      AlternativeStackTrace = alternativeStackTrace;
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
              yield return new StackTraceLine(file, lineNumber, method, false);
            }
            else
            {
              yield return new StackTraceLine(file, null, method, false);
            }
          }
        }
      }

      if (OriginalException == null)
      {
        yield break;
      }

      var exceptionStackTrace = true;
      var stackFrames = new System.Diagnostics.StackTrace(OriginalException, true).GetFrames();

      if (stackFrames == null || stackFrames.Length == 0)
      {
        exceptionStackTrace = false;
        stackFrames = AlternativeStackTrace;
      }

      if (stackFrames == null)
      {
        yield break;
      }

      var seenBugsnagFrames = false;

      foreach (var frame in stackFrames)
      {
        var stackFrame = StackTraceLine.FromStackFrame(frame);

        if (!exceptionStackTrace)
        {
          // if the exception has not come from a stack trace then we need to
          // skip the frames that originate from inside the notifier code base
          var currentStackFrameIsNotify = stackFrame.MethodName.StartsWith("BugsnagUnity.Client.Notify", StringComparison.InvariantCulture);
          seenBugsnagFrames = seenBugsnagFrames || currentStackFrameIsNotify;
          if (!seenBugsnagFrames || currentStackFrameIsNotify)
          {
            continue;
          }
        }

        yield return stackFrame;
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
      var inProject = false;

      return new StackTraceLine(file, lineNumber, methodName, inProject);
    }

    internal StackTraceLine(string file, int? lineNumber, string methodName, bool inProject)
    {
      this.AddToPayload("file", file);
      if (lineNumber.HasValue)
      {
        this.AddToPayload("lineNumber", lineNumber.Value);
      }
      this.AddToPayload("method", methodName);
      this.AddToPayload("inProject", inProject);
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

    internal bool InProject
    {
      get
      {
        return (bool)this.Get("inProject");
      }
      set
      {
        this.AddToPayload("inProject", value);
      }
    }
  }
}
