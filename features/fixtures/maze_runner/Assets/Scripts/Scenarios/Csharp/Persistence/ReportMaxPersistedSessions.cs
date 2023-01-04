using System.Collections;
using System.Collections.Generic;
using System.IO;
using BugsnagUnity;
using UnityEngine;

public class ReportMaxPersistedSessions : Scenario
{
    private bool _sessionsCorrect;

    public override void PrepareConfig(string apiKey, string host)
    {
        _sessionsCorrect = CheckForSessions();
        base.PrepareConfig(apiKey, host);
    }

    private bool CheckForSessions()
    {
        var numSessions = Directory.GetFiles(Application.persistentDataPath + "/Bugsnag/Sessions", "*.session").Length;
        var foundSessions = numSessions == 3;
        if (Directory.Exists(Application.persistentDataPath + "/Bugsnag"))
        {
            Directory.Delete(Application.persistentDataPath + "/Bugsnag", true);
        }
        return foundSessions;
    }

    public override void Run()
    {
        DoSimpleNotify(_sessionsCorrect.ToString().ToLower());
    }
}
