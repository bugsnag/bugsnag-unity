public class IosBundleVersionInConfig : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.BundleVersion = "222";
    }

    public override void Run()
    {
        DoSimpleNotify("IosBundleVersionInConfig");
    }
}
