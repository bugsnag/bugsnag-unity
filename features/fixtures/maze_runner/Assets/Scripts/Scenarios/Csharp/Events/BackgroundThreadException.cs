using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BackgroundThreadException : Scenario
{
    public override void Run()
    {
        var bgThread = new Thread(() => { Debug.LogException(new System.Exception("BackgroundThreadException")); })
        {
            IsBackground = true
        };
        bgThread.Start();
    }
}
