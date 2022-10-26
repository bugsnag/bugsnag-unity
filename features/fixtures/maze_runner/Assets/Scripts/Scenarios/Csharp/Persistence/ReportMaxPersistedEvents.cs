using System.IO;
using UnityEngine;

public class ReportMaxPersistedEvents : Scenario
{

    private bool _eventsCorrect;

    public override void PrepareConfig(string apiKey, string host)
    {
        _eventsCorrect = CheckForEvents();
        base.PrepareConfig(apiKey, host);
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Configuration.EnabledErrorTypes.OOMs = false;
        }
    }

    private bool CheckForEvents()
    {
        var foundEvents = Directory.GetFiles(Application.persistentDataPath + "/Bugsnag/Events", "*.event").Length == 3;
        if (Directory.Exists(Application.persistentDataPath + "/Bugsnag"))
        {
            Directory.Delete(Application.persistentDataPath + "/Bugsnag", true);
        }
        return foundEvents;
    }

    public override void Run()
    {
        DoSimpleNotify(_eventsCorrect.ToString().ToLower());
    }
}



