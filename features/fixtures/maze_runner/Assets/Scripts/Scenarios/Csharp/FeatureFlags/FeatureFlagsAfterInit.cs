using BugsnagUnity;

public class FeatureFlagsAfterInit : Scenario
{
    public override void Run()
    {
        Bugsnag.AddFeatureFlag("testName1", "testVariant1");
        Bugsnag.AddFeatureFlag("testName2", "testVariant2");
        Bugsnag.ClearFeatureFlag("testName1");
        DoSimpleNotify("FeatureFlagsAfterInit");
    }
}
