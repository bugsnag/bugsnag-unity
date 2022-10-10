public class PersistEventReport : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.Context = "Second Error";
    }

    public override void Run()
    {
        throw new System.Exception("Second Error");
    }
}
