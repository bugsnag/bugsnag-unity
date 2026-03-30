using System;
using System.Collections.Generic;
using BugsnagUnity;

public class UnserializableMetadata : Scenario
{
    public override void Run()
    {
        // Add a delegate (which is not JSON-serializable) into metadata
        Action noop = () => { };
        Bugsnag.AddMetadata("unserializable", "testKey", noop);

        DoSimpleNotify("UnserializableMetadata");
    }
}
