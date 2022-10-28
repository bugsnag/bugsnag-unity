﻿using BugsnagUnity;

public class AndroidJVMSmokeTest : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.SetUser("1", "2", "3");
        Configuration.ProjectPackages = new string[] { "test.test.test" };
    }

    public override void Run()
    {
        Bugsnag.LeaveBreadcrumb("test");
        AddTestingMetadata();
        AddTestingFeatureFlags();
        JvmException();
    }
}
