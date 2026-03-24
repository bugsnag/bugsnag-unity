using UnityEngine;
using BugsnagUnity;
using BugsnagUnity.Payload;
using System;

public class EnableBreadcrumbs : Scenario
{

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.EnabledBreadcrumbTypes = new [] { BreadcrumbType.Log };
    }

    public override void Run()
    {
        Debug.Log("Debug.Log");
        Bugsnag.Notify(new Exception("EnableBreadcrumbs 1"));
        throw new Exception("EnableBreadcrumbs 2");
    }
}
