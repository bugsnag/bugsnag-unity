using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using BugsnagUnity;

public class NotifyFromBackgroundThread : Scenario
{
    public override void Run()
    {
        var bgThread = new Thread(() => { Bugsnag.Notify(new System.Exception("NotifyFromBackgroundThread")); })
        {
            IsBackground = true
        };
        bgThread.Start();
    }
}
