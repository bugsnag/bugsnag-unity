using System;
using System.Collections.Generic;
using BugsnagUnity;
using BugsnagUnity.Payload;

public class OnErrorInConfig : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AddOnError((@event) => {

            @event.App.BinaryArch = "BinaryArch";
            @event.App.BundleVersion = "BundleVersion";
            @event.App.CodeBundleId = "CodeBundleId";
            @event.App.DsymUuid = "DsymUuid";
            @event.App.Id = "Id";
            @event.App.ReleaseStage = "ReleaseStage";
            @event.App.Type = "Type";
            @event.App.Version = "Version";
            @event.App.InForeground = false;
            @event.App.IsLaunching = false;

            @event.Device.Id = "Id";
            @event.Device.Jailbroken = true;
            @event.Device.Locale = "Locale";
            @event.Device.Manufacturer = "Manufacturer";
            @event.Device.Model = "Model";
            @event.Device.OsName = "OsName";
            @event.Device.OsVersion = "OsVersion";
            @event.Device.FreeDisk = 123;
            @event.Device.FreeMemory = 123;
            @event.Device.Orientation = "Orientation";

            @event.Errors[0].ErrorClass = "ErrorClass";

            @event.Errors[0].Stacktrace[0].Method = "Method";

            @event.Errors[0].Stacktrace[0].LineNumber = 0;

            foreach (var crumb in @event.Breadcrumbs)
            {
                crumb.Message = "Custom Message";
                crumb.Type = BreadcrumbType.Request;
                crumb.Metadata = new Dictionary<string, object> { { "test", "test" } };
            }

            @event.AddMetadata("test1", new Dictionary<string, object> { { "test", "test" } });
            @event.AddMetadata("test2", new Dictionary<string, object> { { "test", "test" } });
            @event.ClearMetadata("test2");

            @event.AddFeatureFlag("fromCallback","a");

            return true;
        });
    }

    public override void Run()
    {
        Bugsnag.Notify(new Exception("Error 1"));
        throw new Exception("Error 2");
    }
}
