using System.Collections.Generic;
using UnityEngine;
using BugsnagUnity;
using System;

public class ManualAndAutoBreadcrumbs : Scenario
{

    public override void Run()
    {
        Bugsnag.LeaveBreadcrumb(null);
        Bugsnag.LeaveBreadcrumb("Manual");
        Debug.Log("Debug.Log");
        Debug.LogWarning("Debug.LogWarning");
        Debug.LogError("Debug.LogError");
        Debug.LogException(new Exception("Debug.LogException"));
        Bugsnag.Notify(new Exception("FirstError"));
        Bugsnag.LeaveBreadcrumb("Metadata", new Dictionary<string, object>() {
            {"test" , "value" },
            {"nullTest" , null }
        });
        throw new Exception("SecondError");
    }
}
