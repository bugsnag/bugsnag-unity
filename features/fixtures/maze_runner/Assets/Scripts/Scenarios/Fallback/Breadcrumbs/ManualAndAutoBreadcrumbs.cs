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
        Debug.LogException(new Exception("Error1"));
        Bugsnag.Notify(new Exception("Error2"));
        Bugsnag.LeaveBreadcrumb("Metadata", new Dictionary<string, object>() {
            {"test" , "value" },
            {"nullTest" , null }
        });
        throw new Exception("Error3");
    }
}
