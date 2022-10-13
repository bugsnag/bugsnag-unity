public class AppType : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AppType = "custom";
    }

    public override void Run()
    {
        DoSimpleNotify("AppType");
    }
}
