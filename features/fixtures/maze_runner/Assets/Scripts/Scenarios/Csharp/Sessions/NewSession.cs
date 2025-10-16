using System.Collections;
using System.Collections.Generic;
using BugsnagUnity;
using UnityEngine;

public class NewSession : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
    }

    public override void Run()
    {
        Bugsnag.StartSession();
        Bugsnag.Notify(new System.Exception("NewSession 1"));
        Bugsnag.StartSession();
        Bugsnag.Notify(new System.Exception("NewSession 2"));
    }
}
