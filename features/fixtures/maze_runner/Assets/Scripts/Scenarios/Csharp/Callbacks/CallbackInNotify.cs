using BugsnagUnity;

public class CallbackInNotify : Scenario
{
    public override void Run()
    {
        Bugsnag.Notify(new System.Exception("CallbackInNotify 1"),SimpleEventCallback);
    }
}
