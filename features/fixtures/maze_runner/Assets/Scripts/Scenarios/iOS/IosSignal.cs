using BugsnagUnity;

public class IosSignal : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AutoTrackSessions = false;
    }

    public override void Run()
    {
        AddTestingFeatureFlags();
        AddTestingMetadata();
        Bugsnag.LeaveBreadcrumb("test");
        Bugsnag.SetUser("1", "2", "3");
        IosSignal();
    }
}
