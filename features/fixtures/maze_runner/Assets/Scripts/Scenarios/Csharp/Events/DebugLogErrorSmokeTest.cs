using UnityEngine;

public class DebugLogErrorSmokeTest : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.NotifyLogLevel = LogType.Error;
    }

    public override void Run()
    {
        AddTestingMetadata();
        Debug.LogError("DebugLogErrorSmokeTest");
    }
}
