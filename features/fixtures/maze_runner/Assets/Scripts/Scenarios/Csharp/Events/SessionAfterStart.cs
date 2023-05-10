using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionAfterStart : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AutoTrackSessions = true;
    }

    public override void Run()
    {
        throw new System.Exception("SessionAfterStart");
    } 
}
