public class PersistEvent : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetInvalidEndpoints();
        Configuration.Context = "Error 1";
    }

    public override void Run()
    {
        throw new System.Exception("Error 1");
    }
}
