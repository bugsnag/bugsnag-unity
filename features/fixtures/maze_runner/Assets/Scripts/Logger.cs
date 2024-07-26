using System.Collections;
using System;
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
        try
        {
            if (Directory.Exists(Application.persistentDataPath))
                Directory.CreateDirectory(Application.persistentDataPath);
            var path = Path.Combine(Application.persistentDataPath, LOG_FILE_NAME);
            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllText(path, _currentLog);
        }
        catch (UnauthorizedAccessException e)
        {
            Debug.Log(e);
            try
            {
                if (Directory.Exists(Application.dataPath))
                    Directory.CreateDirectory(Application.dataPath);
                var path = Path.Combine(Application.dataPath, LOG_FILE_NAME);
                if (File.Exists(path))
                    File.Delete(path);
                File.WriteAllText(path, _currentLog);
            }
            catch (Exception exc)
            {
                Debug.Log(exc);
            }
        }
    }
   
}
