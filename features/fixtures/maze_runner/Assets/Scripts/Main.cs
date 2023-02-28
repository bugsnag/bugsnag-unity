using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using BugsnagUnity;

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
#endif

#if UNITY_IOS || UNITY_TVOS
    [DllImport("__Internal")]
    private static extern void ClearPersistentData();
#endif

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

#if UNITY_STANDALONE_OSX
        PreventCrashPopups();
#endif
        InvokeRepeating("DoRunNextMazeCommand",0,1);
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
                            ClearUnityCache();
                        }
                        else if ("run_scenario".Equals(command.action))
                        {
                            ScenarioRunner.RunScenario(command.scenarioName, API_KEY, _mazeHost);
                        }
                        else if ("close_application".Equals(command.action))
                        {
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



    // this method never runs and is just used to check that no notify overides are accidentally broken during refactoring
    // if one is broken then the fixture will not compile
    private void OverloadCheck()
    {

        Bugsnag.Notify("name", "message", "stacktrace");

        Bugsnag.Notify("name", "message", "stacktrace", CallBack);

        Bugsnag.Notify(new Exception());

        Bugsnag.Notify(new Exception(), "stacktrace");

        Bugsnag.Notify(new Exception(), "stacktrace", CallBack);

        Bugsnag.Notify(new Exception(), CallBack);

        Bugsnag.Notify(new Exception(), Severity.Error);

        Bugsnag.Notify(new Exception(), Severity.Error, CallBack);
    }

    private bool CallBack(IEvent e)
    {
        return true;
    }

}



