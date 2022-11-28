public class AndroidVersionCodeInConfig : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.VersionCode = 555;
    }

    public override void Run()
    {
        DoSimpleNotify("AndroidVersionCodeInConfig");
    }
}
