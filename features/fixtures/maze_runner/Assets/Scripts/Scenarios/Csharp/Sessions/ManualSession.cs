using BugsnagUnity;

public class ManualSession : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
    }

    public override void Run()
    {
        Bugsnag.StartSession();
    }
}
