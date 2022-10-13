using UnityEngine;

public class DisableUnityLogError : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.EnabledErrorTypes.UnityLog = false;
        Configuration.NotifyLogLevel = LogType.Log;
    }

    public override void Run()
    {
        Debug.Log("Log");
        Debug.LogWarning("Log");
    }
}
