using UnityEngine;

public class AndroidEnableAutomaticContext : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        // Simulate a Unity-managed context value (pipeline-controlled).
        Configuration.Context = "UnityPipelineContext";
    }

    public override void Run()
    {
        // Trigger an error to verify the context matches the Unity-managed value.
        DoSimpleNotify("AndroidEnableAutomaticContext");
    }
}
