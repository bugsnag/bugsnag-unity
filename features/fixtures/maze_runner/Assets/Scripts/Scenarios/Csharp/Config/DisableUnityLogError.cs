public class DisableUnityLogError : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.EnabledErrorTypes.UnityLog = false;
    }

    public override void Run()
    {
        throw new System.Exception("error 2");
    }
}
