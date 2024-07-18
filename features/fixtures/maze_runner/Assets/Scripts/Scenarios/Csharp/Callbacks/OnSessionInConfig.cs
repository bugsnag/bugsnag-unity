using BugsnagUnity;

public class OnSessionInConfig : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AddOnSession(SimpleSessionCallback);
    }

    public override void Run()
    {
        Bugsnag.StartSession();
    }
}
