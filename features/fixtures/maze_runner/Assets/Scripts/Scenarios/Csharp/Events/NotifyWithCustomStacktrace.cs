using BugsnagUnity;
using System;

public class NotifyWithCustomStacktrace : Scenario
{
    public override void Run()
    {
        Bugsnag.Notify(new Exception("NotifyWithCustomStacktrace"), CustomStacktrace);        
    }
}
