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
        Invoke("SendException",1);
    }

    private void SendException()
    {
        DoSimpleNotify("PersistDeviceId");
    }
}
