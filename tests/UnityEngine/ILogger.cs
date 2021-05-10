using System;

namespace UnityEngine
{
  public interface ILogger
  {
    LogType filterLogType { get; }

    bool logEnabled { get; set; }

    ILogHandler logHandler { get; set; }

    bool IsLogTypeAllowed(LogType logType);
    void Log(LogType logType, object message);
    void Log(LogType logType, object message, System.Object context);
    void Log(LogType logType, string tag, object message);
    void Log(LogType logType, string tag, object message, System.Object context);
    void Log(object message);
    void Log(string tag, object message);
    void Log(string tag, object message, System.Object context);
    void LogError(string tag, object message);
    void LogError(string tag, object message, System.Object context);
    void LogException(System.Exception exception);
    void LogFormat(LogType logType, string format, params object[] args);
    void LogWarning(string tag, object message);
    void LogWarning(string tag, object message, System.Object context);
  }
}
