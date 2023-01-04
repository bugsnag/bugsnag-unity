using System.Collections.Generic;
using BugsnagUnity;

public class WebGLTruncation : Scenario
{
    public override void Run()
    {
		var oneHundredCharacters = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";

		for (int i = 0; i < 200; i++)
		{
			var metadata = new Dictionary<string, object>();
			for (int x = 0; x < 100; x++)
			{
				metadata.Add(x.ToString(), oneHundredCharacters);
			}
			Bugsnag.LeaveBreadcrumb(i.ToString(), metadata);
		}

		for (int i = 0; i < 150; i++)
		{
			var section = new Dictionary<string, object>();
			for (int x = 0; x < 100; x++)
			{
				section.Add(x.ToString(), oneHundredCharacters);
			}
			Bugsnag.AddMetadata("m" + i.ToString(), section);
		}

		DoSimpleNotify("WebGLTruncation");
	}
}
