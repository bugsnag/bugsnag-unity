using BugsnagUnity;

public class MacOSNativeCrash : Scenario
{
    public override void Run()
    {
        Bugsnag.LeaveBreadcrumb("test");
        AddTestingMetadata();
        AddTestingFeatureFlags();
        MacOSNativeCrash();
    }
}
