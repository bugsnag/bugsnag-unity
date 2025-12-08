using System;
using BugsnagUnity;

public class OnSendInConfig : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AddOnSendError(SimpleEventCallback);
    }

    public override void Run()
    {
        Bugsnag.Notify(new Exception("OnSendInConfig 1"));
        throw new Exception("OnSendInConfig 2");
    }
}
