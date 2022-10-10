using UnityEngine;

public class DebugLogWarningSmokeTest : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.NotifyLogLevel = LogType.Warning;
    }

    public override void Run()
    {
        AddTestingMetadata();
        Debug.LogWarning("DebugLogWarningSmokeTest");
    }
}
