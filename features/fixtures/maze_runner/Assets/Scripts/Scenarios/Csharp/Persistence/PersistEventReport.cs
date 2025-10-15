using UnityEngine;

public class PersistEventReport : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.Context = "PersistEvent 2";
    }

    public override void Run()
    {
        throw new System.Exception("PersistEvent 2");
    }
}
