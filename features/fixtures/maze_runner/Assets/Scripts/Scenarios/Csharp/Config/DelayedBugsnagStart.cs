using BugsnagUnity;
using UnityEngine;

public class DelayedBugsnagStart : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.LaunchDurationMillis = 6000;
    }

    public override void Run()
    {
        Invoke("DoNotify1", 1);
        Invoke("DoNotify2", 8);
    }

    private void DoNotify1()
    {
        Bugsnag.Notify(new System.Exception("DelayedBugsnagStart 1"));
    }

    private void DoNotify2()
    {
        Bugsnag.Notify(new System.Exception("DelayedBugsnagStart 2"));
    }
}
