using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

[Serializable]
public class Command
{
    public string action;
    public string scenarioName;
}

[Serializable]
public class FixtureConfig
{
    public string maze_address;
}

public class Main : MonoBehaviour
{

#if UNITY_STANDALONE_OSX
    [DllImport("NativeCrashy")]
    private static extern void PreventCrashPopups();
#endif

#if UNITY_IOS || UNITY_TVOS
    [DllImport("__Internal")]
    private static extern void ClearPersistentData();
#endif

    private string _fixtureConfigFileName = "/fixture_config.json";

    private const string API_KEY = "a35a2a72bd230ac0aa0f52715bbdc6aa";
    public static string MazeHost;

    private const int MAX_CONFIG_GET_TRIES = 15;

    public ScenarioRunner ScenarioRunner;

    public IEnumerator Start()
    {
        Log("Maze Runner app started");

        yield return GetFixtureConfig();

#if UNITY_STANDALONE_OSX
        PreventCrashPopups();
#endif
        InvokeRepeating("DoRunNextMazeCommand",0,1);
    }

    private IEnumerator GetFixtureConfig()
    {
        if (Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            var numTries = 0;
            while (numTries < MAX_CONFIG_GET_TRIES)
            {
                var configPath = Application.persistentDataPath + _fixtureConfigFileName;
                if (File.Exists(configPath))
                {
                    var configJson = File.ReadAllText(configPath);
                    Log("Mazerunner got fixture config json: " + configJson);
                    var config = JsonUtility.FromJson<FixtureConfig>(configJson);
                    MazeHost = "http://" + config.maze_address;
                    break;
                }
                else
                {
                    numTries++;
                    Log(string.Format("Maze Runner did not find the config file at path {0}  try number {1}", configPath, numTries));
                    yield return new WaitForSeconds(1);
                }
            }
        }

        if (string.IsNullOrEmpty(MazeHost))
        {
            Log("Host not set from config file, using hard coded");
            MazeHost = "http://localhost:9339";

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.Android)
            {
                MazeHost = "http://bs-local.com:9339";
            }
        }
        Log("Mazerunner host set to: " + MazeHost);
    }

    private void DoRunNextMazeCommand()
    {
        StartCoroutine(RunNextMazeCommand());
    }

    IEnumerator RunNextMazeCommand()
    {
        var url = MazeHost + "/command";
        Log("Requesting maze command from: " + url);
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
            var result = request != null && request.result == UnityWebRequest.Result.Success;
#else
            var result = request != null &&
                !request.isHttpError &&
                !request.isNetworkError;
#endif

            Log("result is " + result);
            if (result)
            {
                var response = request.downloadHandler?.text;
                Log("Raw response: " + response);
                if (response == null || response == "null" || response == "No commands to provide")
                {
                    Log("No Maze Runner command to process at present");
                }
                else
                {
                    var command = JsonUtility.FromJson<Command>(response);
                    if (command != null)
                    {
                        Log("Received Maze Runner command:");
                        Log("Action: " + command.action);
                        Log("Scenario: " + command.scenarioName);

                        if ("clear_cache".Equals(command.action))
                        {
                            ClearUnityCache();
                        }
                        else if ("run_scenario".Equals(command.action))
                        {
                            ScenarioRunner.RunScenario(command.scenarioName, API_KEY, MazeHost);
                        }
                        else if ("close_application".Equals(command.action))
                        {
                            Log("Closing Fixture");
                            CloseFixture();
                        }
                    }
                }
            }
        }
    }

    private void CloseFixture()
    {
        Application.Quit();
    }


    private void ClearUnityCache()
    {
#if UNITY_SWITCH
        return;
#endif
        if (Directory.Exists(Application.persistentDataPath + "/Bugsnag"))
        {
            Directory.Delete(Application.persistentDataPath + "/Bugsnag",true);
        }
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            ClearIOSData();
        }
        if (Application.platform != RuntimePlatform.Android &&
            Application.platform != RuntimePlatform.IPhonePlayer)
        {
            Invoke("CloseFixture", 0.25f);
        }
    }

    public static void ClearIOSData()
    {
#if UNITY_IOS
        ClearPersistentData();
#endif
    }

    private static void Log(string msg)
    {
        Logger.I(msg);
    }

}



