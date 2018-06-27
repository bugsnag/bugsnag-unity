using System.Collections.Generic;
using UnityEngine;

namespace Bugsnag.Unity
{
  /// <summary>
  /// The Bugsnag API defines these three severities
  /// </summary>
  public enum Severity
  {
    Info = 0,

    Warning = 1,

    Error = 2
  }

  static class LogTypeExtensions
  {
    /// <summary>
    /// The LogType enum from Unity doesn't have its enum values in an expected
    /// numerical order. This allows us to map them to something that we would
    /// expect.
    /// </summary>
    private static Dictionary<LogType, int> LogTypeMapping { get; } = new Dictionary<LogType, int>
    {
      { LogType.Log, 0 },
      { LogType.Warning, 1 },
      { LogType.Assert, 2 },
      { LogType.Error, 3 },
      { LogType.Exception, 4 },
    };

    internal static bool IsGreaterThanOrEqualTo(this LogType logType, LogType log)
    {
      return LogTypeMapping[logType] >= LogTypeMapping[log];
    }
  }
}
