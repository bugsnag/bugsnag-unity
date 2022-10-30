using System;
using BugsnagUnity;

public class OnErrorAfterStart : Scenario
{
    public override void Run()
    {
        Bugsnag.AddOnError(SimpleEventCallback);
        Bugsnag.Notify(new Exception("Error 1"));
        throw new Exception("Error 2");
    }

   
}