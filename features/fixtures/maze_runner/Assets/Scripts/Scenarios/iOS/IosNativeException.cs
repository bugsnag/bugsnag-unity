using BugsnagUnity;

public class IosNativeException : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
    }

    public override void Run()
    {
        AddTestingFeatureFlags();
        AddTestingMetadata();
        Bugsnag.LeaveBreadcrumb("test");
        Bugsnag.SetUser("1","2","3");
        IosException();
    }
}
