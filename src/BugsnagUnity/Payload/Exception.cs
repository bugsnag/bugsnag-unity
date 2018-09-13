using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BugsnagUnity.Payload
{
  class UnityLogExceptions : IEnumerable<Exception>
  {
    UnityLogMessage UnityLogMessage { get; }

    System.Diagnostics.StackFrame[] AlternativeStackTrace { get; }

    internal UnityLogExceptions(UnityLogMessage logMessage, System.Diagnostics.StackFrame[] alternativeStackTrace)
    {
      UnityLogMessage = logMessage;
      AlternativeStackTrace = alternativeStackTrace;
    }

    public IEnumerator<Exception> GetEnumerator()
    {
      yield return Exception.FromUnityLogMessage(UnityLogMessage, AlternativeStackTrace);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }

  /// <summary>
  /// Represents a set of Bugsnag payload exceptions that are generated from a single exception by resolving
  /// the inner exceptions present.
  /// </summary>
  class Exceptions : IEnumerable<Exception>
  {
    private IEnumerable<Exception> UnwoundExceptions { get; }

    internal Exceptions(System.Exception exception, System.Diagnostics.StackFrame[] alternativeStackTrace)
    {
      UnwoundExceptions = FlattenAndReverseExceptionTree(exception).Select(e => Exception.FromSystemException(e, alternativeStackTrace));
    }

    public IEnumerator<Exception> GetEnumerator()
    {
      return UnwoundExceptions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    private static IEnumerable<System.Exception> FlattenAndReverseExceptionTree(System.Exception ex)
    {
      if (ex == null) yield break;

      yield return ex;

      switch (ex)
      {
        case ReflectionTypeLoadException typeLoadException:
          foreach (var exception in typeLoadException.LoaderExceptions)
          {
            foreach (var item in FlattenAndReverseExceptionTree(exception))
            {
              yield return item;
            }
          }
          break;
        default:
          foreach (var item in FlattenAndReverseExceptionTree(ex.InnerException))
          {
            yield return item;
          }
          break;
      }
    }
  }

  /// <summary>
  /// Represents an individual exception in the Bugsnag payload.
  /// </summary>
  public class Exception : Dictionary<string, object>
  {
    internal Exception(string errorClass, string message, StackTraceLine[] stackTrace)
    {
      this.AddToPayload("errorClass", errorClass);
      this.AddToPayload("message", message);
      this.AddToPayload("stacktrace", stackTrace);
    }

    internal IEnumerable<StackTraceLine> StackTrace { get { return this.Get("stacktrace") as IEnumerable<StackTraceLine>; } }

    public string ErrorClass => this.Get("errorClass") as string;

    public string ErrorMessage => this.Get("message") as string;

    internal static Exception FromSystemException(System.Exception exception, System.Diagnostics.StackFrame[] alternativeStackTrace)
    {
      var errorClass = TypeNameHelper.GetTypeDisplayName(exception.GetType());
      var stackFrames = new System.Diagnostics.StackTrace(exception, true).GetFrames();

      StackTraceLine[] lines = null;

      if (stackFrames != null && stackFrames.Length > 0)
      {
        lines = new StackTrace(stackFrames).ToArray();
      }
      else
      {
        lines = new StackTrace(alternativeStackTrace).ToArray();
      }

      return new Exception(errorClass, exception.Message, lines);
    }

    internal static Exception FromUnityLogMessage(UnityLogMessage logMessage, System.Diagnostics.StackFrame[] stackFrames)
    {
      var match = Regex.Match(logMessage.Condition, @"^(?<errorClass>\S+):\s*(?<message>.*)", RegexOptions.Singleline);

      var lines = new StackTrace(stackFrames).ToArray();

      if (match.Success)
      {
        return new Exception(match.Groups["errorClass"].Value, match.Groups["message"].Value.Trim(), lines);
      }
      else
      {
        // include the type somehow in there
        return new Exception($"UnityLog{logMessage.Type}", logMessage.Condition, lines);
      }
    }
  }
}
