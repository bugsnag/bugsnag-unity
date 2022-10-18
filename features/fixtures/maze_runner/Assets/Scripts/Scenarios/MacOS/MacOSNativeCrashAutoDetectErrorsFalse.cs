using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MacOSNativeCrashAutoDetectErrorsFalse : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AutoDetectErrors = false;
    }

    public override void Run()
    {
        MacOSNativeCrash();
    }
}
