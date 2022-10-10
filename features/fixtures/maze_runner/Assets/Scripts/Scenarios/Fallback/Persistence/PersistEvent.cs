public class PersistEvent : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetInvalidEndpoints();
        Configuration.Context = "First Error";
    }

    public override void Run()
    {
        throw new System.Exception("First Error");
    }
}
