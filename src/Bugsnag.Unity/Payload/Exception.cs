using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Bugsnag.Unity.Payload
{
  public class UnityLogExceptions : IEnumerable<Exception>
  {
    private UnityLogMessage UnityLogMessage { get; }

    public UnityLogExceptions(UnityLogMessage logMessage)
    {
      UnityLogMessage = logMessage;
    }

    public IEnumerator<Exception> GetEnumerator()
    {
      yield return Exception.FromUnityLogMessage(UnityLogMessage);
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
  public class Exceptions : IEnumerable<Exception>
  {
    private IEnumerable<Exception> UnwoundExceptions { get; }

    public Exceptions(System.Exception exception)
    {
      UnwoundExceptions = FlattenAndReverseExceptionTree(exception).Select(e => Exception.FromSystemException(e));
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
    public Exception(string errorClass, string message, StackTraceLine[] stackTrace)
    {
      this.AddToPayload("errorClass", errorClass);
      this.AddToPayload("message", message);
      this.AddToPayload("stacktrace", stackTrace);
    }

    public IEnumerable<StackTraceLine> StackTrace { get { return this.Get("stacktrace") as IEnumerable<StackTraceLine>; } }

    public string ErrorClass { get { return this.Get("errorClass") as string; } }

    public string ErrorMessage { get { return this.Get("message") as string; } }

    internal static Exception FromSystemException(System.Exception exception)
    {
      var errorClass = TypeNameHelper.GetTypeDisplayName(exception.GetType());
      var stackTrace = new StackTrace(exception).ToArray();
      return new Exception(errorClass, exception.Message, stackTrace);
    }

    internal static Exception FromUnityLogMessage(UnityLogMessage logMessage)
    {
      var match = Regex.Match(logMessage.Condition, @"^(?<errorClass>\S+):\s*(?<message>.*)", RegexOptions.Singleline);

      if (match.Success)
      {
        return new Exception(match.Groups["errorClass"].Value, match.Groups["message"].Value.Trim(), new StackTrace(logMessage.StackTrace).ToArray());
      }
      else
      {
        // include the type somehow in there
        return new Exception($"UnityLog{logMessage.Type}", logMessage.Condition, new StackTrace(logMessage.StackTrace).ToArray());
      }
    }
  }
}
