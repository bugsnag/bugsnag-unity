using System.Collections;
using System.Collections.Generic;
using BugsnagUnity;
using UnityEngine;
using System.IO;
using System;

public class Test : MonoBehaviour
{

    private const int MAX_CONFIG_GET_TRIES = 15;
    private string _fixtureConfigFileName = "/fixture_config.json";
    public static string MazeHost;


    [Serializable]
    public class FixtureConfig
    {
        public string maze_address;
    }

    private IEnumerator GetFixtureConfig()
    {

        var numTries = 0;
        while (numTries < MAX_CONFIG_GET_TRIES)
        {
            var configPath = Application.persistentDataPath + _fixtureConfigFileName;
            if (File.Exists(configPath))
            {
                var configJson = File.ReadAllText(configPath);
                var config = JsonUtility.FromJson<FixtureConfig>(configJson);
                MazeHost = "http://" + config.maze_address;
                break;
            }
            else
            {
                numTries++;
                yield return new WaitForSeconds(1);
            }
        }

        if (string.IsNullOrEmpty(MazeHost))
        {
            MazeHost = "http://bs-local.com:9339";
        }
    }


    const string LOG_FILE_NAME = "mazerunner-unity.log";

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return GetFixtureConfig();
        var path = Path.Combine(Application.persistentDataPath, LOG_FILE_NAME);
        File.WriteAllText(path, "EDM Started");
        Bugsnag.Start(GetDefaultConfig());
        throw new System.Exception("EDM4U");
    }

    private Configuration GetDefaultConfig()
    {
        Configuration config = new Configuration("12312312312312312312312312312312");
        config.Endpoints = new EndpointConfiguration(MazeHost + "/notify", MazeHost + "/sessions");
        return config;
    }

}
