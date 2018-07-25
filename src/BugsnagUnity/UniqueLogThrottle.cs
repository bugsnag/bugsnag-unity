using System;
using System.Collections.Generic;

namespace BugsnagUnity
{
  /// <summary>
  /// Applies the configured unique log throttling rules.
  /// </summary>
  public class UniqueLogThrottle
  {
    private readonly object _lock = new object();

    /// <summary>
    /// Used to track the unique messages received.
    /// </summary>
    private Dictionary<UnityLogMessage, int> Counter { get; }

    /// <summary>
    /// Used to track when the counter should be flushed next
    /// </summary>
    private DateTime FlushAt { get; set; }

    /// <summary>
    /// The configuration for unique log counts and times
    /// </summary>
    private IConfiguration Configuration { get; }

    private TimeSpan UniqueLogsTimePeriod => Configuration.UniqueLogsTimePeriod;

    public UniqueLogThrottle(IConfiguration configuration)
    {
      Configuration = configuration;
      Counter = new Dictionary<UnityLogMessage, int>(new UnityLogMessageEqualityComparer());
      FlushAt = DateTime.UtcNow.Add(Configuration.UniqueLogsTimePeriod);
    }

    /// <summary>
    /// Determines if this log message should be sent to Bugsnag based on the
    /// configured rules around unique log messages with a time period.
    /// </summary>
    /// <param name="unityLogMessage"></param>
    /// <returns></returns>
    public bool ShouldSend(UnityLogMessage unityLogMessage)
    {
      bool shouldSend;

      lock (_lock)
      {
        shouldSend = !Counter.ContainsKey(unityLogMessage);

        if (shouldSend)
        {
          Counter.Add(unityLogMessage, 0);
        }
        else
        {
          if (unityLogMessage.CreatedAt > FlushAt)
          {
            Counter.Clear();
            FlushAt = DateTime.UtcNow.Add(UniqueLogsTimePeriod);
            shouldSend = true;
          }
        }
      }

      return shouldSend;
    }

    /// <summary>
    /// Used to determine if log messages are unique.
    /// </summary>
    class UnityLogMessageEqualityComparer : EqualityComparer<UnityLogMessage>
    {
      public override bool Equals(UnityLogMessage x, UnityLogMessage y)
      {
        return x.Condition == y.Condition
          && x.StackTrace == y.StackTrace
          && x.Type == y.Type;
      }

      public override int GetHashCode(UnityLogMessage obj)
      {
        unchecked // Overflow is fine, just wrap
        {
          int hash = 17;
          hash = hash * 23 + obj.Condition.GetHashCode();
          hash = hash * 23 + obj.StackTrace.GetHashCode();
          hash = hash * 23 + obj.Type.GetHashCode();
          return hash;
        }
      }
    }
  }
}
