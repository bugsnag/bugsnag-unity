using System.Collections.Generic;
using BugsnagUnity;
using UnityEngine;

public class BreadcrumbTruncation : Scenario
{
    public override void Run()
    {
        var largeString = "";
        for (int i = 0; i < 10000; i++)
        {
            largeString += "1";
        }

        for (int i = 0; i < 100; i++)
        {
            Bugsnag.LeaveBreadcrumb(i.ToString(), new Dictionary<string, object>() { { "1", largeString } });
        }

        DoSimpleNotify("BreadcrumbTruncation");
    }
}

