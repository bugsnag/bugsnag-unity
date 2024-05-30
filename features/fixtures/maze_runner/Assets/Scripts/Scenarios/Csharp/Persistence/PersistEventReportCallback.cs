using System.Collections.Generic;
using UnityEngine;

public class PersistEventReportCallback : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.Context = "Error 2";
        Configuration.AddOnSendError((@event) => {

            @event.App.BinaryArch = "Persist BinaryArch";

            @event.Device.Id = "Persist Id";

            @event.Errors[0].ErrorClass = "Persist ErrorClass";

            @event.Errors[0].Stacktrace[0].Method = "Persist Method";

            foreach (var crumb in @event.Breadcrumbs)
            {
                crumb.Message = "Persist Message";
            }

            @event.AddMetadata("Persist Section", new Dictionary<string, object> { { "Persist Key", "Persist Value" } });

            return true;
        });
    }

    public override void Run()
    {
        throw new System.Exception("Error 2");
    }
}
