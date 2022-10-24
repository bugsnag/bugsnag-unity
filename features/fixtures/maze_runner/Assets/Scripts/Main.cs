using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using BugsnagUnity;
using BugsnagUnity.Payload;
using UnityEngine.SceneManagement;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class Command
{
    public string action;
    public string scenarioName;
}

public class Main : MonoBehaviour
{

#if UNITY_STANDALONE_OSX
    [DllImport("NativeCrashy")]
    private static extern void PreventCrashPopups();

    [DllImport("NativeCrashy")]
    private static extern void crashy_signal_runner(float num);
#endif

#if UNITY_SWITCH

    [DllImport("__Internal")]
    private static extern int bugsnag_getArgsCount();

    [DllImport("__Internal")]
    private static extern string bugsnag_getArg(int index);

    private SwitchCacheType _switchCacheType = SwitchCacheType.R;
    private int _switchCacheIndex = 0;
    private string _switchCacheMountName = "BugsnagCache";
#endif

    public Text DebugText;

    private const string API_KEY = "a35a2a72bd230ac0aa0f52715bbdc6aa";
    private string _mazeHost;

    public ScenarioRunner ScenarioRunner;

    public void Start()
    {
        Debug.Log("Maze Runner app started");


        _mazeHost = "http://localhost:9339";

        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.Android)
        {
            _mazeHost = "http://bs-local.com:9339";
        }

        GetSwitchArguments();
#if UNITY_STANDALONE_OSX
        PreventCrashPopups();
#endif
        InvokeRepeating("DoRunNextMazeCommand",0,1);
    }

    // example command: RunOnTarget.exe 0x01004B9000490000 --no-wait -- --mazeIp 192.168.0.whatever --cacheType i --cacheIndex 3 --cacheMountName BugsnagCache
    private void GetSwitchArguments()
    {
#if UNITY_SWITCH
        int count = bugsnag_getArgsCount();
        Debug.Log("args count: " + count);

        for (int i = 0; i < count; i++)
        {
            var arg = bugsnag_getArg(i);
            if (!arg.Contains("--"))
            {
                Debug.Log("Ignoring arg: " + arg);
                continue;
            }
            Debug.Log("CHECKING ARG: " + arg);
            switch (arg)
            {
                case "--mazeIp":
                    var ip = bugsnag_getArg(i + 1);
                    _mazeHost = "http://" + ip + ":9339";
                    Debug.Log("SET MAZE HOST TO: " + _mazeHost);
                    break;

                case "--cacheType":
                    var cacheType = bugsnag_getArg(i + 1);
                    switch (cacheType)
                    {
                        case "r":
                            _switchCacheType = SwitchCacheType.R;
                            break;
                        case "i":
                            _switchCacheType = SwitchCacheType.I;
                            break;
                        case "n":
                            _switchCacheType = SwitchCacheType.None;
                            break;
                        default:
                            var msg = ("Unknown cacheType option: " + cacheType);
                            throw new Exception(msg);
                    }
                    Debug.Log("Switch Cache Type set to: " + _switchCacheType);
                    break;

                case "--cacheIndex":
                    _switchCacheIndex = int.Parse(bugsnag_getArg(i + 1));
                    Debug.Log("Switch cache index set to: " + _switchCacheIndex);
                    break;

                case "--cacheMountName":
                    _switchCacheMountName = bugsnag_getArg(i + 1);
                    Debug.Log("Switch cache mount name set to: " + _switchCacheMountName);
                    break;
            }
        }
#endif
    }

    private void DoRunNextMazeCommand()
    {
        StartCoroutine(RunNextMazeCommand());
    }

    IEnumerator RunNextMazeCommand()
    {
        var url = _mazeHost + "/command";
        Console.WriteLine("RunNextMazeCommand called, requesting command from: {0}", url);
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

            Console.WriteLine("result is " + result);
            if (result)
            {
                var response = request.downloadHandler?.text;
                Console.WriteLine("Raw response: " + response);
                if (response == null || response == "null" || response == "No commands to provide")
                {
                    Console.WriteLine("No Maze Runner command to process at present");
                }
                else
                { 
                    var command = JsonUtility.FromJson<Command>(response);
                    if (command != null)
                    {
                        Console.WriteLine("Received Maze Runner command:");
                        Console.WriteLine("Action: " + command.action);
                        Console.WriteLine("Scenario: " + command.scenarioName);

                        if ("clear_cache".Equals(command.action))
                        {
                            DebugText.text = command.action;
                            ClearUnityCache();
                        }
                        else if ("run_scenario".Equals(command.action))
                        {
                            DebugText.text = command.scenarioName;
                            ScenarioRunner.RunScenario(command.scenarioName, API_KEY, _mazeHost);
                        }
                        else if ("close_application".Equals(command.action))
                        {
                            Application.Quit();
                        }
                    }
                }
            }
        }
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
            MobileNative.ClearIOSData();
        }
    }

}



