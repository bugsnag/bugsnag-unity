﻿using System.Collections.Generic;
using System.Text.RegularExpressions;

public class RedactedKeys : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.RedactedKeys = new List<Regex> { new Regex("testKey") };
        Configuration.AddMetadata("testSection","testKey","testValue");
    }

    public override void Run()
    {
        DoSimpleNotify("RedactedKeys");
    }
}
