using System.Collections;
using System.Collections.Generic;
using BugsnagUnity;
using UnityEngine;

public class MaxPersistSessions : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.MaxPersistedSessions = 3;
        Configuration.AutoTrackSessions = false;
    }

    public override void Run()
    {
        StartCoroutine("NotifyPersistedSessions");
    }

    private IEnumerator NotifyPersistedSessions()
    {
        for (int i = 0; i < 4; i++)
        {
            Bugsnag.StartSession();
            yield return new WaitForSeconds(1f);
        }
    }
}
