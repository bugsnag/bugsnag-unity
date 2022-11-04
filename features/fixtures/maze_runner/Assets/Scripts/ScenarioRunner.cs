using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioRunner : MonoBehaviour
{

#if UNITY_SWITCH

    [DllImport("__Internal")]
    private static extern int bugsnag_getArgsCount();

    [DllImport("__Internal")]
    private static extern string bugsnag_getArg(int index);

    private SwitchCacheType _switchCacheType = SwitchCacheType.R;
    private int _switchCacheIndex = 0;
    private string _switchCacheMountName = "BugsnagCache";
#endif

    public void RunScenario(string scenarioName, string apiKey, string host)
    {

        var scenario = GetScenario(scenarioName);
        scenario.PrepareConfig(apiKey,host);
#if UNITY_SWITCH

        GetSwitchArguments();
        scenario.AddSwitchConfigValues(_switchCacheType, _switchCacheIndex, _switchCacheMountName);
#endif
        scenario.StartBugsnag();
        scenario.Run();
    }

    private Scenario GetScenario(string scenarioName)
    {

        var scenarios = gameObject.GetComponentsInChildren<Scenario>();

        foreach (var scenario in scenarios)
        {
            if (scenario.GetType().Name.Equals(scenarioName))
            {
                return scenario;
            }
        }

        throw new System.Exception("Scenario not found: " + scenarioName);
    }

#if UNITY_SWITCH

    // example command: RunOnTarget.exe 0x01004B9000490000 --no-wait -- --mazeIp 192.168.0.whatever --cacheType i --cacheIndex 3 --cacheMountName BugsnagCache
    private void GetSwitchArguments()
    {
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
}
#endif

}
