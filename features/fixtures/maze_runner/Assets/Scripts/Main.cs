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
    public string uuid;
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
    private string _commandUuidFileName = "/command_uuid.txt";

    private const string API_KEY = "a35a2a72bd230ac0aa0f52715bbdc6aa";
    public static string MazeHost;
    private static string LastCommandUuid;

    private const int MAX_CONFIG_GET_TRIES = 15;

    public ScenarioRunner ScenarioRunner;

    public IEnumerator Start()
    {
        Log("Maze Runner app started");

        GetLastCommandUuid();

        yield return GetFixtureConfig();

#if UNITY_STANDALONE_OSX
        PreventCrashPopups();
#endif
        InvokeRepeating("DoRunNextMazeCommand", 0, 1);
    }

    private void GetLastCommandUuid()
    {
        var uuidFilePath = Application.persistentDataPath + _commandUuidFileName;
        if (File.Exists(uuidFilePath))
        {
            LastCommandUuid = File.ReadAllText(uuidFilePath);
        }
        else
        {
            LastCommandUuid = "";
        }
        Log("Last command UUID is: " + LastCommandUuid);
    }

    private void SetLastCommandUuid(String uuid) 
    {
        Log("Setting last command UUID: " + uuid);
        var uuidFilePath = Application.persistentDataPath + _commandUuidFileName;
        File.WriteAllText(uuidFilePath, uuid);
        LastCommandUuid = uuid;
        Log("Command UUID is now: " + LastCommandUuid);
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
        Log("Maze Runner host set to: " + MazeHost);
    }

    private void DoRunNextMazeCommand()
    {
        StartCoroutine(RunNextMazeCommand());
    }

    IEnumerator RunNextMazeCommand()
    {
        var url = MazeHost + "/idem-command?after=" + LastCommandUuid;
        Log("Requesting maze command from: " + url);
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            var result = request != null && request.result == UnityWebRequest.Result.Success;

            if (result)
            {
                var response = request.downloadHandler?.text;
                if (response == null || response == "null")
                {
                    Log("No Maze Runner command to process at present");
                }
                else
                {
                    var command = JsonUtility.FromJson<Command>(response);
                    if (command != null)
                    {
                        Log("Received Maze Runner command:\n" + response);

                        switch(command.action) 
                        {
                            case "noop":
                                break;
                            case "reset_uuid":
                                SetLastCommandUuid("");
                                break;
                            case "clear_cache":
                                ClearUnityCache();
                                SetLastCommandUuid(command.uuid);
                                break;
                            case "run_scenario":
                                SetLastCommandUuid(command.uuid);
                                ScenarioRunner.RunScenario(command.scenarioName, API_KEY, MazeHost);
                                break;
                            case "close_application":
                                SetLastCommandUuid(command.uuid);
                                CloseFixture();
                                break;
                        }
                    }
                }
            }
            else 
            {
                Log("Request error: " + request.error);
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

        Log("Start ClearUnityCache");
#if DEBUG
        ListAllPersistentDataFiles();
#endif

        if (Directory.Exists(Application.persistentDataPath + "/Bugsnag"))
        {
            Directory.Delete(Application.persistentDataPath + "/Bugsnag", true);
        }
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            ClearIOSData();
        }
#if DEBUG
        ListAllPersistentDataFiles();
#endif
        Log("End ClearUnityCache");
    }

    private void ListAllPersistentDataFiles()
    {
        string rootPath = Application.persistentDataPath;
        string listing = "Contents of " + rootPath + ":\n";
        string[] files = Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            listing += "  " + file + "\n";
        }
        Log(listing);
    }

    public static void ClearIOSData()
    {
#if UNITY_IOS
        ClearPersistentData();
#endif
    }

    private static void Log(string msg)
    {
        try
        {
            Logger.I(msg);
        }
        catch
        {
            // can fail on windows
        }
    }
}
