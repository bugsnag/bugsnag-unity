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
    /// Used to stop the dictionary boxing the value types
    /// </summary>
    class LogTypeComparer : IEqualityComparer<LogType>
    {
      public bool Equals(LogType x, LogType y)
      {
        return x == y;
      }

      public int GetHashCode(LogType obj)
      {
        return (int)obj;
      }
    }

    /// <summary>
    /// The LogType enum from Unity doesn't have its enum values in an expected
    /// numerical order. This allows us to map them to something that we would
    /// expect.
    /// </summary>
    private static Dictionary<LogType, int> LogTypeMapping { get; } = new Dictionary<LogType, int>(5, new LogTypeComparer())
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
