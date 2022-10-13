public class FeatureFlagsInConfig : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AddFeatureFlag("testName1", "testVariant1");
        Configuration.AddFeatureFlag("testName2", "testVariant2");
        Configuration.ClearFeatureFlag("testName1");
    }

    public override void Run()
    {
        DoSimpleNotify("FeatureFlagsInConfig");
    }
}
