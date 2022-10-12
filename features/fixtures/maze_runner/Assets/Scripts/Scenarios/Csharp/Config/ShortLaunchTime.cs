using System.Collections;
using System.Collections.Generic;
using BugsnagUnity;
using UnityEngine;

public class ShortLaunchTime : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.LaunchDurationMillis = 3000;
    }

    public override void Run()
    {
        Invoke("DoNotify1", 2);
        Invoke("DoNotify2", 5);
    }

    private void DoNotify1()
    {
        Bugsnag.Notify(new System.Exception("Error 1"));
    }

    private void DoNotify2()
    {
        Bugsnag.Notify(new System.Exception("Error 2"));
    }
}
