using UnityEngine;

namespace BugsnagUnity.Payload.Tests
{
    class StubLogger : ILogger
    {
      public bool logEnabled { get; set; } = false;
      public ILogHandler logHandler { get; set; } = null;
      public LogType filterLogType { get; } = LogType.Error;
      public StubLogger() {}
      public bool IsLogTypeAllowed(LogType logType) { return false; }
      public void Log(LogType logType, object message){}
      public void Log(LogType logType, object message, System.Object context){}
      public void Log(LogType logType, string tag, object message){}
      public void Log(LogType logType, string tag, object message, System.Object context){}
      public void Log(object message){}
      public void Log(string tag, object message){}
      public void Log(string tag, object message, System.Object context){}
      public void LogError(string tag, object message){}
      public void LogError(string tag, object message, System.Object context){}
      public void LogException(System.Exception exception){}
      public void LogFormat(LogType logType, string format, params object[] args){}
      public void LogWarning(string tag, object message){}
      public void LogWarning(string tag, object message, System.Object context){}
    }
}
