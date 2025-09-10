using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BugsnagUnity;

public class GroupingDiscriminator : Scenario
{
    public override void Run()
    {
        Bugsnag.Notify(new System.Exception("GroupingDiscriminator-1"));
        Bugsnag.GroupingDiscriminator = "Global GroupingDiscriminator";
        Bugsnag.Notify(new System.Exception("GroupingDiscriminator-2"));
        Bugsnag.Notify(new System.Exception("GroupingDiscriminator-3"), (@event) =>
        {
            @event.GroupingDiscriminator = "Callback GroupingDiscriminator";
            return true;
        });
    }
}
