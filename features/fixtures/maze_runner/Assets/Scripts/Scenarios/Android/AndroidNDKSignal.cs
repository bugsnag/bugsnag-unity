using BugsnagUnity;

public class AndroidNDKSignal : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.Context = "My Context";
    }

    public override void Run()
    {
        Bugsnag.LeaveBreadcrumb("test");
        AddTestingMetadata();
        AddTestingFeatureFlags();
        RaiseNdkSignal();
    }
}
