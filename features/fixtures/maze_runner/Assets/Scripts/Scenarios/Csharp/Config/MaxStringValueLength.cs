﻿using BugsnagUnity;

public class MaxStringValueLength : Scenario
{

    private string _oneHundredCharacters = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.MaxStringValueLength = 20;
        for (int i = 0; i < 10000; i++)
        {
            Configuration.AddMetadata("test", "key-" + i.ToString(), _oneHundredCharacters);
        }
    }

    public override void Run()
    {
        Bugsnag.LeaveBreadcrumb("truncated",new System.Collections.Generic.Dictionary<string, object>()
        {
            {"testKey", _oneHundredCharacters }
        });
        DoSimpleNotify("MaxStringValueLength");
    }
}
