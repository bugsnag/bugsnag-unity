using System.Collections.Generic;

public class MetadataInConfig : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AddMetadata("dictionary", new Dictionary<string, object>(){
            {"foo", "bar" },
        });

        Configuration.AddMetadata("stringArray", "testKey", new string[] { "1", "2", "3" });

        Configuration.AddMetadata("numberArray", "testKey", new int[] { 1, 2, 3 });

        Configuration.AddMetadata("string", "testKey", "testValue");

        Configuration.AddMetadata("string", "testKey2", "testValue2");

        Configuration.AddMetadata("number", "testKey", 123);

        Configuration.AddMetadata("toClear", "toClear", "toClear");

        Configuration.ClearMetadata("toClear");

        Configuration.ClearMetadata("string", "testKey");
    }

    public override void Run()
    {
        DoSimpleNotify("MetadataInConfig");
    }
}
