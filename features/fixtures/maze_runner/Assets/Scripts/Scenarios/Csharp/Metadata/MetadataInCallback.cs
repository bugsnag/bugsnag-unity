using System.Collections.Generic;

public class MetadataInCallback : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AddOnError((@event)=> {

            @event.AddMetadata("dictionary", new Dictionary<string, object>(){
                {"foo", "bar" },
            });

            @event.AddMetadata("stringArray", "testKey", new string[] { "1", "2", "3" });

            @event.AddMetadata("numberArray", "testKey", new int[] { 1, 2, 3 });

            @event.AddMetadata("string", "testKey", "testValue");

            @event.AddMetadata("string", "testKey2", "testValue2");

            @event.AddMetadata("number", "testKey", 123);

            @event.AddMetadata("toClear", "toClear", "toClear");

            @event.ClearMetadata("toClear");

            @event.ClearMetadata("string", "testKey");

            return true;
        });
    }

    public override void Run()
    {
        DoSimpleNotify("MetadataInCallback");
    }
}
