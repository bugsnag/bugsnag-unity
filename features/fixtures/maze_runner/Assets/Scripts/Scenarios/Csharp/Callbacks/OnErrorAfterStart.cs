using System;
using BugsnagUnity;

public class OnErrorAfterStart : Scenario
{
    public override void Run()
    {
        Bugsnag.AddOnError(SimpleCallback);
        Bugsnag.Notify(new Exception("Error 1"));
        throw new Exception("Error 2");
    }

   
}