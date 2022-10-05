using System;
using System.Collections;
using System.Collections.Generic;
using BugsnagUnity;
using UnityEngine;

public class SetUserInConfig : Scenario
{

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.SetUser("1", "2", "3");
    }

    public override void Run()
    {
        base.Run();
        Bugsnag.Notify(new Exception("SetUserInConfig"));
    }

}
