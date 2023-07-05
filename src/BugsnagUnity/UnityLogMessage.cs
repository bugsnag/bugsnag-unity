using System;
using UnityEngine;

namespace BugsnagUnity
{
    /// <summary>
    /// Represents a log message received from Unity
    /// </summary>
    public class UnityLogMessage
    {
        public UnityLogMessage(string condition, string stackTrace, LogType type)
        {
            CreatedAt = Client.BSTime;
            Condition = condition;
            StackTrace = stackTrace;
            Type = type;
        }

        public UnityLogMessage(Exception exception)
        {
            CreatedAt = Client.BSTime;
            Condition = exception.Message == null ? string.Empty : exception.Message;
            StackTrace = exception.StackTrace == null ? string.Empty : exception.StackTrace;
            Type = LogType.Exception;
        }

        public string Condition { get; }

        public string StackTrace { get; }

        public LogType Type { get; }

        public double CreatedAt { get; }

        
    }
}
