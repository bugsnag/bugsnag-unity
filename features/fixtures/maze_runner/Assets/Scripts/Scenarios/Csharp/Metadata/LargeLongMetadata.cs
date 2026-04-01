using System.Collections.Generic;
using BugsnagUnity;

public class LargeLongMetadata : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
    }

    public override void Run()
    {
        // Add various long values to metadata including values that overflow Int64
        Bugsnag.AddMetadata("numeric", new Dictionary<string, object>(){
            {"validLong", 9223372036854775807L }, // Int64.MaxValue
            {"negativeLong", -9223372036854775808L }, // Int64.MinValue
            {"largeLong", 12345678901234567890UL }, // Exceeds Int64.MaxValue (UInt64)
            {"normalInt", 42 },
            {"normalDouble", 123.456 }
        });

        Bugsnag.LeaveBreadcrumb("Test large long values");
        
        DoSimpleNotify("LargeLongMetadata");
    }
}
