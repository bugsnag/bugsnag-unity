using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BugsnagUnity;
using System;

public class SetUserAfterStart : Scenario
{

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
    }

    public override void Run()
    {
        base.Run();
        Bugsnag.SetUser("1", "2", "3");
        Bugsnag.Notify(new Exception("SetUserAfterStart"));
    }

}
