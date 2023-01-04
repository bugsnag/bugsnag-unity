using System;
using BugsnagUnity;

public class UncaughtExceptionSmokeTest : Scenario
{
    public override void Run()
    {
        AddTestingMetadata();
        throw new Exception("UncaughtExceptionSmokeTest");
    }
}
