using System.Collections;
using System.Collections.Generic;
using BugsnagUnity;
using UnityEngine;

public class MultipleEventCounts : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.ReportExceptionLogsAsHandled = false;
    }

    public override void Run()
    {
        Bugsnag.StartSession();
        Bugsnag.Notify(new System.Exception("MultipleEventCounts Handled Error 1"));
        Bugsnag.Notify(new System.Exception("MultipleEventCounts Handled Error 2"));
        throw new System.Exception("MultipleEventCounts Unhandled Error 1");
    }
}
