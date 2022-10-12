public class AutoNotifyFalse : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AutoDetectErrors = false;
    }

    public override void Run()
    {
        throw new System.Exception("Error 1");
    }
}
