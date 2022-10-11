using System;
using BugsnagUnity;

public class OnSendInConfig : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AddOnSendError(SimpleCallback);
    }

    public override void Run()
    {
        Bugsnag.Notify(new Exception("Error 1"));
        throw new Exception("Error 2");
    }
}
