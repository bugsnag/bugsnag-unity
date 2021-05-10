using System;

namespace UnityEngine
{
  public interface ILogHandler
  {
    void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args);

    void LogException(Exception exception, UnityEngine.Object context);
  }
}
