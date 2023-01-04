using BugsnagUnity;

public class MacOSNativeCrash : Scenario
{

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.SetUser("1", "2", "3");
    }

    public override void Run()
    {
        Bugsnag.LeaveBreadcrumb("test");
        AddTestingMetadata();
        AddTestingFeatureFlags();
        MacOSNativeCrash();
    }
}
