using System;
using UnityEngine;

namespace BugsnagUnity
{
  /// <summary>
  /// Represents a log message received from Unity
  /// </summary>
  public class UnityLogMessage
  {

    private const int NUM_STACKTRACE_LINES_TO_TRIM = 4;

    public UnityLogMessage(string condition, string stackTrace, LogType type)
    {
      CreatedAt = DateTime.UtcNow;
      Condition = condition;
      StackTrace = stackTrace;
      Type = type;
    }

    public UnityLogMessage(Exception exception)
    {
        CreatedAt = DateTime.UtcNow;
        Condition = exception.Message;
        if (exception.StackTrace == null)
        {
            var generatedStackTrace = new System.Diagnostics.StackTrace(NUM_STACKTRACE_LINES_TO_TRIM).ToString();
            StackTrace = generatedStackTrace;
        }
        else
        {
            StackTrace = exception.StackTrace;
        }
        Type = LogType.Exception;
    }
       
    public string Condition { get; }

    public string StackTrace { get; }

    public LogType Type { get; }

    public DateTime CreatedAt { get; }
  }
}
