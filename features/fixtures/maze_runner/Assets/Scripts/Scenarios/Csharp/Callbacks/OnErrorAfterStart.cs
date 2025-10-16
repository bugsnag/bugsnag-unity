using System;
using BugsnagUnity;

public class OnErrorAfterStart : Scenario
{
    public override void Run()
    {
        Bugsnag.AddOnError(SimpleEventCallback);
        Bugsnag.Notify(new Exception("OnErrorAfterStart 1"));
        throw new Exception("OnErrorAfterStart 2");
    }

   
}