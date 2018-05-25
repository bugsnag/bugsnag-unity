using System;
using UnityEngine;

namespace Bugsnag.Unity
{
  /// <summary>
  /// Represents a log message received from Unity
  /// </summary>
  public class UnityLogMessage
  {
    public UnityLogMessage(string condition, string stackTrace, LogType type)
    {
      CreatedAt = DateTime.UtcNow;
      Condition = condition;
      StackTrace = stackTrace;
      Type = type;
    }

    public string Condition { get; }

    public string StackTrace { get; }

    public LogType Type { get; }

    public DateTime CreatedAt { get; }
  }
}
