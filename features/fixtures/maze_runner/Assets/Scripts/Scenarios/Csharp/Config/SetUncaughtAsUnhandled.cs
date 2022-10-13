using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetUncaughtAsUnhandled : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.ReportExceptionLogsAsHandled = false;
    }

    public override void Run()
    {
        Debug.LogException(new System.Exception("SetUncaughtAsUnhandled"));
    }
}
