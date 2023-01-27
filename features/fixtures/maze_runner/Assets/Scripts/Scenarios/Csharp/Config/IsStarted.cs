using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BugsnagUnity;

public class IsStarted : Scenario
{

    public override void Run()
    {
        if (Bugsnag.IsStarted())
        {
            DoSimpleNotify("IsStarted");
        }
        else
        {
            DoSimpleNotify("HasNotStarted");
        }
    }
}
