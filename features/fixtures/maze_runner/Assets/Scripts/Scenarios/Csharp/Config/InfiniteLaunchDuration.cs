using System.Collections;
using System.Collections.Generic;
using BugsnagUnity;
using UnityEngine;

public class InfiniteLaunchDuration : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.LaunchDurationMillis = 0;
    }

    public override void Run()
    {
        Invoke("DoException",10);
    }

    private void DoException()
    {
        Bugsnag.Notify(new System.Exception("InfiniteLaunchDuration"));
    }
}
