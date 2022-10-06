using System;
using UnityEngine;

public class DebugLogExceptionSmokeTest : Scenario
{
    public override void Run()
    {
        AddTestingMetadata();
        Debug.LogException(new Exception("DebugLogExceptionSmokeTest"));
    }
}
