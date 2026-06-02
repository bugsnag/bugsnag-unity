using System;
using BugsnagUnity;

public class DeviceStateSmokeTest : Scenario
{
    public override void Run()
    {
        try
        {
            throw new Exception("DeviceStateSmokeTest");
        }
        catch (System.Exception e)
        {
            Bugsnag.Notify(e);
        }
    }
}
