﻿using BugsnagUnity;

public class PausedSessionEvent : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
    }

    public override void Run()
    {
        Bugsnag.StartSession();
        Bugsnag.PauseSession();
        Bugsnag.Notify(new System.Exception("PausedSessionEvent"));
    }
}
