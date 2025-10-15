using UnityEngine;

public class PersistEvent : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.Context = "PersistEvent 1";
    }

    public override void Run()
    {
        throw new System.Exception("PersistEvent 1");
    }
}
