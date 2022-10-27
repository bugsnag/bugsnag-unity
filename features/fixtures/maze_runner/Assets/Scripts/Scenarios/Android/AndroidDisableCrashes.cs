using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidDisableCrashes : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {

        base.PrepareConfig(apiKey, host);
        Configuration.EnabledErrorTypes.Crashes = false;
    }

    public override void Run()
    {
        TriggerBackgroundJVMException();
    }
}
