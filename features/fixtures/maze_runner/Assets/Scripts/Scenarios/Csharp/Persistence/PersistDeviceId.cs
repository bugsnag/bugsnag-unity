using UnityEngine;

public class PersistDeviceId : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Configuration.EnabledErrorTypes.OOMs = false;
        }
    }

    public override void Run()
    {
        throw new System.Exception("PersistDeviceId");
    }
}
