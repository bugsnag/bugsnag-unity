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

    public string Condition { get; }

    public string StackTrace { get; }

    public LogType Type { get; }

    public DateTime CreatedAt { get; }

    public bool IsFromSystemException;

    public UnityLogMessage(string condition, string stackTrace, LogType type)
    {
      CreatedAt = DateTime.UtcNow;
      Condition = condition;
      StackTrace = stackTrace;
      Type = type;
      IsFromSystemException = false;
    }

    public UnityLogMessage(Exception exception)
    {
        CreatedAt = DateTime.UtcNow;
        Condition = FormatSystemCondition(exception);
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
        IsFromSystemException = true;
    }

    private string FormatSystemCondition(Exception exception)
    {
        var type = exception.GetType().ToString().Replace("System.",string.Empty);
        return string.Format("{0}: {1}", type, exception.Message);
    }

    
  }
}
