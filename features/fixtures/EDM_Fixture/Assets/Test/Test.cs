using System.Collections;
using System.Collections.Generic;
using BugsnagUnity;
using UnityEngine;
using System.IO;

public class Test : MonoBehaviour
{

    const string LOG_FILE_NAME = "mazerunner-unity.log";

    // Start is called before the first frame update
    void Start()
    {
        var path = Path.Combine(Application.persistentDataPath, LOG_FILE_NAME);
        File.WriteAllText(path, "EDM Started");
        Bugsnag.Start(GetDefaultConfig());
        throw new System.Exception("EDM4U");
    }

    private Configuration GetDefaultConfig()
    {
        Configuration config = new Configuration("12312312312312312312312312312312");
        config.Endpoints = new EndpointConfiguration("http://bs-local.com:9339/notify", "http://bs-local.com:9339/sessions");
        return config;
    }
   
}
