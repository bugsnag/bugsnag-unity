using BugsnagUnity;
using System;
public class MaxBreadcrumbs : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.MaximumBreadcrumbs = 5;
    }

    public override void Run()
    {
        for (int i = 0; i < 10; i++)
        {
            Bugsnag.LeaveBreadcrumb("Crumb " + i);
        }
        Bugsnag.Notify(new Exception("MaxBreadcrumbs"));
    }
}
