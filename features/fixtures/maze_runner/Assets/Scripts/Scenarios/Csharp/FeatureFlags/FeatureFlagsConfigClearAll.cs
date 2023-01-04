public class FeatureFlagsConfigClearAll : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AddFeatureFlag("testName1", "testVariant1");
        Configuration.AddFeatureFlag("testName2", "testVariant2");
        Configuration.ClearFeatureFlags();
    }

    public override void Run()
    {
        DoSimpleNotify("FeatureFlagsConfigClearAll");
    }
}
