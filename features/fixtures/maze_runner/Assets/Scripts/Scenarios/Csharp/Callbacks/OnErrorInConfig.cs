using System;
using BugsnagUnity;

public class OnErrorInConfig : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AddOnError(SimpleCallback);
    }

    public override void Run()
    {
        Bugsnag.Notify(new Exception("Error 1"));
        throw new Exception("Error 2");
    }
}
