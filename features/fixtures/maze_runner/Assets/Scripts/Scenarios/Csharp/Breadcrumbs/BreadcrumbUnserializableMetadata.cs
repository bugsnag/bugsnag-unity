using System;
using System.Collections.Generic;
using UnityEngine;
using BugsnagUnity;
using BugsnagUnity.Payload;

public class BreadcrumbUnserializableMetadata : Scenario
{
    public override void Run()
    {
        // Create a simple unserializable value (Action delegate)
        Action testDelegate = () => { Debug.Log("delegate"); };
        
        // Create metadata with unserializable value
        var metadata = new Dictionary<string, object>
        {
            { "unserializable", testDelegate }
        };

        Bugsnag.LeaveBreadcrumb("Test breadcrumb", metadata, BreadcrumbType.Manual);
        DoSimpleNotify("breadcrumb-unserializable");
    }
}
