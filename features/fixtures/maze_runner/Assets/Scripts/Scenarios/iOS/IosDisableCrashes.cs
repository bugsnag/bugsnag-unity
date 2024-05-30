using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IosDisableCrashes : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.EnabledErrorTypes.Crashes = false;
        Configuration.EnabledErrorTypes.OOMs = true;
    }

    public override void Run()
    {
        IosException();
    }
}
