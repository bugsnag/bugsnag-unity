using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCacheNone : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.SwitchCacheType = BugsnagUnity.SwitchCacheType.None;
    }

    public override void Run()
    {
        DoSimpleNotify("SwitchCacheNone");
    }
}
