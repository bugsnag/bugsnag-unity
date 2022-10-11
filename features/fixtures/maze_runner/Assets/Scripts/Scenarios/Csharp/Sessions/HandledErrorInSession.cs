using BugsnagUnity;

public class HandledErrorInSession : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AutoTrackSessions = false;
    }

    public override void Run()
    {
        Bugsnag.StartSession();
        Bugsnag.Notify(new System.Exception("HandledErrorInSession"));
    }
}
