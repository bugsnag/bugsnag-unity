using BugsnagUnity;

public class NotifyWithStrings : Scenario
{
    public override void Run()
    {
        Bugsnag.Notify("name", "NotifyWithStrings", CustomStacktrace);
    }
}
