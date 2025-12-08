using System;
using BugsnagUnity;

public class OnErrorInConfig : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AddOnError(SimpleEventCallback);
    }

    public override void Run()
    {
        Bugsnag.Notify(new Exception("OnErrorInConfig 1"));
        throw new Exception("OnErrorInConfig 2");
    }
}
