using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BugsnagUnity;
using System;

public class EnableBreadcrumbs : Scenario
{

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.EnabledBreadcrumbTypes = new BugsnagUnity.Payload.BreadcrumbType[] {BugsnagUnity.Payload.BreadcrumbType.Log };
    }

    public override void Run()
    {
        base.Run();
        Debug.Log("Debug.Log");
        Bugsnag.Notify(new Exception("FirstError"));
        throw new Exception("EnableBreadcrumbs");
    }
}
