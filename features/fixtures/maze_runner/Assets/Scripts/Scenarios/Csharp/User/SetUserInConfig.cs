using System;
using BugsnagUnity;

public class SetUserInConfig : Scenario
{

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.SetUser("1", "2", "3");
    }

    public override void Run()
    {
        Bugsnag.Notify(new Exception("SetUserInConfig"));
    }

}
