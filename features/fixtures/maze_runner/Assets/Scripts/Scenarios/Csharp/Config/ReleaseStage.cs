using BugsnagUnity;

public class ReleaseStage : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.EnabledReleaseStages = new string[] {"release"};
        Configuration.ReleaseStage = "notRelease";
    }

    public override void Run()
    {
        Bugsnag.Notify(new System.Exception("ReleaseStage"));
    }
}
