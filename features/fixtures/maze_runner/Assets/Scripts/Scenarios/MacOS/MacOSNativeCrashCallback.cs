using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MacOSNativeCrashCallback : Scenario
{

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AddOnSendError(CocoaNativeEventCallback);
    }

  
}
