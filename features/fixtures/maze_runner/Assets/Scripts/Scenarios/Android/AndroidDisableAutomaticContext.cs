using UnityEngine;

public class AndroidDisableAutomaticContext : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        // Explicitly set null context so the Android notifier is placed into MANUAL
        // context mode and cannot overwrite context from Activity lifecycle events.
        Configuration.Context = null;
    }

    public override void Run()
    {
        // Trigger an error to verify context remains empty (not set to Activity name)
        DoSimpleNotify("AndroidDisableAutomaticContext");
    }
}
