using System.Collections;
using System.Collections.Generic;
using BugsnagUnity;
using UnityEngine;

public class SwitchMaxCacheSize : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.SwitchCacheMaxSize = 2097155;
        Configuration.AutoTrackSessions = false;
    }

    public override void Run()
    {
        StartCoroutine(DoMaxSwitchCacheTest());
    }

    private IEnumerator DoMaxSwitchCacheTest()
    {
        CacheLargePayload(1);
        yield return new WaitForSeconds(1);
        CacheLargePayload(2);
    }

    private void CacheLargePayload(int index)
    {
        Bugsnag.Notify(new System.Exception("LARGE PAYLOAD " + index), report =>
        {
            for (int i = 0; i < 100000; i++)
            {
                report.AddMetadata("test", "test" + i, "test");
            }
            return true;
        });
    }
}
