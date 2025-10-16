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
        InvokeRepeating("DoRunNextMazeCommand", 0, 1);
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
                            ClearCache();
                        }
                        else if ("run_scenario".Equals(command.action))
                        {
                            ScenarioRunner.RunScenario(command.scenarioName, API_KEY, MazeHost);
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

    private void ClearCache()
    {
#if UNITY_SWITCH
        return;
#endif
        ClearUnityCache();
        if (Application.platform == RuntimePlatform.Android)
        {
            ClearAndroidCache();
            return;
        }
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            ClearIOSCache();
            return;
        }
        Invoke("CloseFixture", 0.25f);
    }

    private void ClearUnityCache()
    {
        DeleteTargets(Application.persistentDataPath, new[] { "Bugsnag" }, Array.Empty<string>());
    }

    public static void ClearIOSCache()
    {
#if UNITY_IOS
        ClearPersistentData();
#endif
    }

    private void ClearAndroidCache()
    {
#if UNITY_ANDROID
        try
        {
            string cacheRoot;
            string filesRoot;
            GetAndroidRoots(out cacheRoot, out filesRoot);

            DeleteTargets(cacheRoot, new[] { "bugsnag", "StrictModeDiscScenarioFile" }, Array.Empty<string>());
            DeleteTargets(filesRoot, new[] { "background-service-dir" }, new[] { "device-id", "internal-device-id" });

            ClearAndroidSharedPreferences("com.bugsnag.android");

            ListFolder(cacheRoot, "CACHE");
            ListFolder(filesRoot, "FILES");
        }
        catch (Exception e)
        {
            Log($"[Cleaner] Failed to clear Android persistent data: {e}");
        }
#endif
    }

#if UNITY_ANDROID
    private static void GetAndroidRoots(out string cacheRoot, out string filesRoot)
    {
        cacheRoot = null;
        filesRoot = null;
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        {
            using (var cacheDir = activity.Call<AndroidJavaObject>("getCacheDir"))
            {
                cacheRoot = cacheDir.Call<string>("getAbsolutePath");
            }
            using (var filesDir = activity.Call<AndroidJavaObject>("getFilesDir"))
            {
                filesRoot = filesDir.Call<string>("getAbsolutePath");
            }
        }
    }

    private void ClearAndroidSharedPreferences(string name)
    {
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var prefs = activity.Call<AndroidJavaObject>("getSharedPreferences", name, 0))
            using (var editor = prefs.Call<AndroidJavaObject>("edit"))
            {
                editor.Call<AndroidJavaObject>("clear").Call<bool>("commit");
            }
        }
        catch (Exception e)
        {
            Log($"[Cleaner] Failed to clear SharedPreferences '{name}': {e}");
        }
    }
#endif

    private static void DeleteTargets(string root, string[] dirs, string[] files)
    {
        foreach (var d in dirs)
        {
            var p = Path.Combine(root, d);
            DeleteDirectoryIfExists(p);
        }
        foreach (var f in files)
        {
            var p = Path.Combine(root, f);
            DeleteFileIfExists(p);
        }
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                LogStatic($"[Cleaner] Deleting dir: {path}");
                Directory.Delete(path, true);
            }
            else
            {
                LogStatic($"[Cleaner] Dir not found (skip): {path}");
            }
        }
        catch (Exception e)
        {
            LogStatic($"[Cleaner] Could not delete dir {path}: {e.Message}");
        }
    }

    private static void DeleteFileIfExists(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                LogStatic($"[Cleaner] Deleting file: {path}");
                File.Delete(path);
            }
            else
            {
                LogStatic($"[Cleaner] File not found (skip): {path}");
            }
        }
        catch (Exception e)
        {
            LogStatic($"[Cleaner] Could not delete file {path}: {e.Message}");
        }
    }

    private static void ListFolder(string root, string label)
    {
        try
        {
            LogStatic($"[Cleaner] Contents of {label} root: {root}");
            if (!Directory.Exists(root))
            {
                LogStatic($"[Cleaner] Root does not exist: {root}");
                return;
            }
            foreach (var entry in Directory.EnumerateFileSystemEntries(root, "*", SearchOption.AllDirectories))
            {
                LogStatic(entry);
            }
        }
        catch (Exception e)
        {
            LogStatic($"[Cleaner] Could not list {label} root {root}: {e.Message}");
        }
    }

    private static void LogStatic(string msg)
    {
        try { Logger.I(msg); } catch { }
    }

    private static void Log(string msg)
    {
        try { Logger.I(msg); } catch { }
    }
}