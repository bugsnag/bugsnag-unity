using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateAnonymousId : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.GenerateAnonymousId = false;
    }

    public override void Run()
    {
        DoSimpleNotify("GenerateAnonymousId");
    }
}
