using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Bugsnag.Unity.Payload
{
  /// <summary>
  /// Represents a set of Bugsnag payload stacktrace lines that are generated from a single StackTrace provided
  /// by the runtime.
  /// </summary>
  class StackTrace : IEnumerable<StackTraceLine>
  {
    private readonly System.Exception _originalException;

    private readonly string _originalStackTrace;

    private static string[] StringSplit { get; } = { Environment.NewLine };

    /// <summary>
    /// Looks for lines that have matching parentheses. This indicates that
    /// the line contains a method call.
    /// </summary>
    private static Regex StackTraceLineRegex { get; } = new Regex(@"(?<method>\S+\s*\(.*?\))\s*(?:(?:\[.*\]\s*in\s|\(at\s*\s*)(?<file>.*):(?<linenumber>\d+))?");

    internal StackTrace(string stackTrace)
    {
      _originalStackTrace = stackTrace;
    }

    internal StackTrace(System.Exception exception)
    {
      _originalException = exception;
    }

    public IEnumerator<StackTraceLine> GetEnumerator()
    {
      if (_originalStackTrace != null)
      {
        foreach (var item in _originalStackTrace.Split(StringSplit, StringSplitOptions.RemoveEmptyEntries))
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

      if (_originalException == null)
      {
        yield break;
      }

      var exceptionStackTrace = true;
      var stackFrames = new System.Diagnostics.StackTrace(_originalException, true).GetFrames();

      if (stackFrames == null)
      {
        // this usually means that the exception has not been thrown so we need
        // to try and create a stack trace at the point that the notify call
        // was made.
        exceptionStackTrace = false;
        stackFrames = new System.Diagnostics.StackTrace(true).GetFrames();
      }

      if (stackFrames == null)
      {
        yield break;
      }

      bool seenBugsnagFrames = false;

      foreach (var frame in stackFrames)
      {
        var stackFrame = StackTraceLine.FromStackFrame(frame);

        if (!exceptionStackTrace)
        {
          // if the exception has not come from a stack trace then we need to
          // skip the frames that originate from inside the notifier code base
          var currentStackFrameIsNotify = stackFrame.MethodName.StartsWith("Bugsnag.Client.Notify");
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
