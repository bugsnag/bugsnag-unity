using BugsnagUnity;
using BugsnagUnity.Payload;
using System;

public class DisableBreadcrumbs : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.EnabledBreadcrumbTypes = new BreadcrumbType[0];
    }

    public override void Run()
    {
        Bugsnag.Notify(new Exception("DisableBreadcrumbs"));
    }
}
