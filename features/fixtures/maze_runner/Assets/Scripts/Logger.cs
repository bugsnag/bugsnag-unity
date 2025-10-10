using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Logger : MonoBehaviour
{
    const string LOG_PREFIX = "BUGSNAG_MAZERUNNER_LOG : ";

    const string LOG_FILE_NAME = "mazerunner-unity.log";

    public static void I(string msg)
    {
        Debug.Log(LOG_PREFIX + msg);

        var path = Path.Combine(Application.persistentDataPath, LOG_FILE_NAME);
        var log = string.Format("[{0:yyyy-MM-dd HH:mm:ss.fff}] {1}\n", DateTime.Now, msg);
        File.AppendAllText(path, log);
    }
}
