using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeatureFlagsInCallback : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AddFeatureFlag("testName1", "testVariant1");
        Configuration.AddFeatureFlag("testName2", "testVariant1");
        Configuration.AddFeatureFlag("testName2", "testVariant2");
        Configuration.ClearFeatureFlag("testName1");
        Configuration.AddOnSendError((@event) => {
            @event.AddFeatureFlag("testName3", "testVariant3");
            return true;
        });
    }

    public override void Run()
    {
        DoSimpleNotify("FeatureFlagsInCallback");
    }
}
