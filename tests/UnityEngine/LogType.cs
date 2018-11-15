namespace UnityEngine
{
  //
  // Summary:
  //     The type of the log message in Debug.unityLogger.Log or delegate registered with
  //     Application.RegisterLogCallback.
  public enum LogType
  {
    //
    // Summary:
    //     LogType used for Errors.
    Error = 0,
    //
    // Summary:
    //     LogType used for Asserts. (These could also indicate an error inside Unity itself.)
    Assert = 1,
    //
    // Summary:
    //     LogType used for Warnings.
    Warning = 2,
    //
    // Summary:
    //     LogType used for regular log messages.
    Log = 3,
    //
    // Summary:
    //     LogType used for Exceptions.
    Exception = 4
  }
}
