using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Logger : MonoBehaviour
{

    const string LOG_PREFIX = "BUGSNAG_MAZERUNNER_LOG : ";

    const string LOG_FILE_NAME = "mazerunner-unity.log";

    private static string _currentLog;

    public static void I(string msg)
    {
        Debug.Log(LOG_PREFIX + msg);
        _currentLog += msg + "\n";
        WriteLogFile();
    }

    private static void WriteLogFile()
    {
        var path = Path.Combine(Application.persistentDataPath, LOG_FILE_NAME);
        File.WriteAllText(path, _currentLog);
    }
   
}
