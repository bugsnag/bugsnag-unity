using BugsnagUnity;

public class CallbackInNotify : Scenario
{
    public override void Run()
    {
        Bugsnag.Notify(new System.Exception("Error 1"),SimpleEventCallback);
    }
}
