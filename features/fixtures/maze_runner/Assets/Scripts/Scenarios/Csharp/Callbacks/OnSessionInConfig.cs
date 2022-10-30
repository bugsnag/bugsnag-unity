using BugsnagUnity;

public class OnSessionInConfig : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AddOnSession(SimpleSessionCallback);
        Configuration.AutoTrackSessions = false;
    }

    public override void Run()
    {
        Bugsnag.StartSession();
    }
}
