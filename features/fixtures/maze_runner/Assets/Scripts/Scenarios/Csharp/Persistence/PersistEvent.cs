using UnityEngine;

public class PersistEvent : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetInvalidEndpoints();
        Configuration.Context = "Error 1";
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Configuration.EnabledErrorTypes.OOMs = false;
        }
    }

    public override void Run()
    {
        throw new System.Exception("Error 1");
    }
}
