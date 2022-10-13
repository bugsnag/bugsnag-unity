using BugsnagUnity;

public class FeatureFlagsAfterInitClearAll : Scenario
{
    public override void Run()
    {
        Bugsnag.AddFeatureFlag("testName1", "testVariant1");
        Bugsnag.AddFeatureFlag("testName2", "testVariant2");
        Bugsnag.ClearFeatureFlags();
        DoSimpleNotify("FeatureFlagsAfterInitClearAll");
    }
}
