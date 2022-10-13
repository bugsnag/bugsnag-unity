using UnityEngine;

public class DisableUnityLogError : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.EnabledErrorTypes.UnityLog = false;
    }

    public override void Run()
    {
        Debug.LogException(new System.Exception("Error 1"));
    }
}
