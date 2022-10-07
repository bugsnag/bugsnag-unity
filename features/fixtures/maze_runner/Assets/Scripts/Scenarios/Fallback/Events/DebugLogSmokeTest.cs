using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLogSmokeTest : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.NotifyLogLevel = LogType.Log;
    }

    public override void Run()
    {
        AddTestingMetadata();
        Debug.Log("DebugLogSmokeTest");
    }
}
