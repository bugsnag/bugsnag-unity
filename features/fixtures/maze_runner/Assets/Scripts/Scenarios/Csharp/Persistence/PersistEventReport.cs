using UnityEngine;

public class PersistEventReport : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.Context = "Error 2";
    }

    public override void Run()
    {
        throw new System.Exception("Error 2");
    }
}
