using UnityEngine;

public class ReportMaxPersistedEvents : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Configuration.EnabledErrorTypes.OOMs = false;
        }
    }
}
