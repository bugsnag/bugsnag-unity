using BugsnagUnity;

public class IosLastRunInfo : Scenario
{

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
    }

    public override void Run()
    {
        var info = Bugsnag.GetLastRunInfo();
        if (info.Crashed && info.CrashedDuringLaunch && info.ConsecutiveLaunchCrashes > 0)
        {
            DoSimpleNotify("Last Run Info Correct");
        }
        else
        {
            DoSimpleNotify("Last Run Info Not Correct");
        }
    }
}
