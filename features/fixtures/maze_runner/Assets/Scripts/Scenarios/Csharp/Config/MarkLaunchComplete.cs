﻿using BugsnagUnity;

public class MarkLaunchComplete : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.LaunchDurationMillis = 0;
    }

    public override void Run()
    {
        Invoke("DoException", 10);
    }

    private void DoException()
    {
        Bugsnag.Notify(new System.Exception("Error 1"));
        Bugsnag.MarkLaunchCompleted();
        Bugsnag.Notify(new System.Exception("Error 2"));
    }
}
