using System;
using System.Collections;
using BugsnagUnity;
using UnityEngine;

public class MaxPersistEvents : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetInvalidEndpoints();
        Configuration.MaxPersistedEvents = 3;
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Configuration.EnabledErrorTypes.OOMs = false;
        }
    }

    public override void Run()
    {
        StartCoroutine("NotifyPersistedEvents");
    }

    private IEnumerator NotifyPersistedEvents()
    {
        for (int i = 0; i < 5; i++)
        {
            Bugsnag.Notify(new Exception("Error " + i));
            yield return new WaitForSeconds(2f);
        }
    }
}
