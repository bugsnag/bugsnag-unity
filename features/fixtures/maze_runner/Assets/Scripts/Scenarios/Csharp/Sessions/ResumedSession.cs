using System.Collections;
using System.Collections.Generic;
using BugsnagUnity;
using UnityEngine;

public class ResumedSession : Scenario
{

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
    }

    public override void Run()
    {
        Bugsnag.StartSession();
        Bugsnag.Notify(new System.Exception("ResumedSession 1"));
        Bugsnag.PauseSession();
        Bugsnag.Notify(new System.Exception("ResumedSession 2"));
        Bugsnag.ResumeSession();
        Bugsnag.Notify(new System.Exception("ResumedSession 3"));
    }

}
