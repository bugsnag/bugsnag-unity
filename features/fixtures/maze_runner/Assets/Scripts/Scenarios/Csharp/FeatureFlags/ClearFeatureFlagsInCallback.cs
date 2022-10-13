using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearFeatureFlagsInCallback : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AddOnSendError((@event) => {
            @event.AddFeatureFlag("testName3", "testVariant3");
            @event.ClearFeatureFlags();
            return true;
        });
    }

    public override void Run()
    {
        DoSimpleNotify("ClearFeatureFlagsInCallback");
    }
}
