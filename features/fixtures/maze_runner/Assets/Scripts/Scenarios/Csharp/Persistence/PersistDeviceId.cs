using UnityEngine;

public class PersistDeviceId : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
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
