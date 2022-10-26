using UnityEngine;

public class PersistEventReport : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.Context = "Error 2";
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Configuration.EnabledErrorTypes.OOMs = false;
        }
    }

    public override void Run()
    {
        throw new System.Exception("Error 2");
    }
}
