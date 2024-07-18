using BugsnagUnity;

public class OnSessionAfterStart : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
    }

    public override void Run()
    {
        Bugsnag.AddOnSession(SimpleSessionCallback);
        Bugsnag.StartSession();
    }
}
