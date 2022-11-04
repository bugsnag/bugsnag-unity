using BugsnagUnity;

public class LongLaunchTime : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.LaunchDurationMillis = 7000;
    }

    public override void Run()
    {
        Invoke("DoNotify1",6);
        Invoke("DoNotify2", 8);
    }

    private void DoNotify1()
    {
        Bugsnag.Notify(new System.Exception("Error 1"));
    }

    private void DoNotify2()
    {
        Bugsnag.Notify(new System.Exception("Error 2"));
    }
}
