using System.Collections.Generic;
using BugsnagUnity;

public class MetadataAfterStart : Scenario
{
    public override void Run()
    {
        Bugsnag.AddMetadata("dictionary", new Dictionary<string, object>(){
            {"foo", "bar" },
        });

        Bugsnag.AddMetadata("stringArray", "testKey", new string[] { "1", "2", "3" });

        Bugsnag.AddMetadata("numberArray", "testKey", new int[] { 1, 2, 3 });

        Bugsnag.AddMetadata("string", "testKey", "testValue");

        Bugsnag.AddMetadata("string", "testKey2", "testValue2");

        Bugsnag.AddMetadata("number", "testKey", 123);

        Bugsnag.AddMetadata("toClear", "toClear", "toClear");

        Bugsnag.ClearMetadata("toClear");

        Bugsnag.ClearMetadata("string", "testKey");

        DoSimpleNotify("MetadataAfterStart");

    }

  
}
