using BugsnagUnity;

public class CustomSeverity : Scenario
{
    public override void Run()
    {
        Bugsnag.Notify(new System.Exception("CustomSeverity"), Severity.Info);
    }
}
