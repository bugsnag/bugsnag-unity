using System;
using BugsnagUnity;

public class NotifySmokeTest : Scenario
{
    public override void Run()
    {
        AddTestingMetadata();
        try
        {
          throw new Exception("NotifySmokeTest");
        }
        catch (System.Exception e)
        {
          Bugsnag.Notify(e);
        }
    }
}
