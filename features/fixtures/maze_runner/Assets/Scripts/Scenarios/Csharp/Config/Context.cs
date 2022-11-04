public class Context : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.Context = "test";
    }

    public override void Run()
    {
        DoSimpleNotify("Context");
    }
}
