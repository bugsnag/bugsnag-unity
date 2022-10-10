using System;
using BugsnagUnity;

public class NotifySmokeTest : Scenario
{
    public override void Run()
    {
        AddTestingMetadata();
        Bugsnag.Notify(new Exception("NotifySmokeTest"));
    }
}
