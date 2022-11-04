using BugsnagUnity;

public class DiscardErrorClass : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.DiscardClasses = new string[] { "IndexOutOfRangeException" };
    }

    public override void Run()
    {
        Bugsnag.Notify(new System.IndexOutOfRangeException("Error 1"));
        Bugsnag.Notify(new System.Exception("Error 2"));
    }
}
