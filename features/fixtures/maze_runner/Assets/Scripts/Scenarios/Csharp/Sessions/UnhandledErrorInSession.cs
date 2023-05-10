using BugsnagUnity;

public class UnhandledErrorInSession : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.ReportExceptionLogsAsHandled = false;
    }

    public override void Run()
    {
        Bugsnag.StartSession();
        throw new System.Exception("UnhandledErrorInSession");
    }
}
