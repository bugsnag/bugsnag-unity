using System;
using System.Collections;
using System.Collections.Generic;
using BugsnagUnity;
using UnityEngine;

public class NotifySmokeTest : Scenario
{
    public override void Run()
    {
        AddTestingMetadata();
        Bugsnag.Notify(new Exception("NotifySmokeTest"));
    }
}
