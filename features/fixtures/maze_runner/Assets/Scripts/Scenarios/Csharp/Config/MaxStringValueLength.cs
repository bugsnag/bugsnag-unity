using System.Collections.Generic;
using BugsnagUnity;

public class MaxStringValueLength : Scenario
{

    private string _oneHundredCharacters = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.MaxStringValueLength = 20;
        for (int i = 0; i < 10000; i++)
        {
            Configuration.AddMetadata("test", "key-" + i.ToString(), _oneHundredCharacters);
        }
        Configuration.AddMetadata("test", "stringArray", new string[] { _oneHundredCharacters });
        Configuration.AddMetadata("test", "stringList", new List<string>() { _oneHundredCharacters });


    }

    public override void Run()
    {
        Bugsnag.LeaveBreadcrumb("truncated",new Dictionary<string, object>()
        {
            {"testKey", _oneHundredCharacters },
            {"stringArray", new string[] { _oneHundredCharacters }},
            {"stringList", new List<string>() { _oneHundredCharacters }}
        });
        DoSimpleNotify("MaxStringValueLength");
    }
}
