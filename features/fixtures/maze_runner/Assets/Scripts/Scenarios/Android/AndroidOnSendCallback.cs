using System;
using System.Collections;
using System.Collections.Generic;
using BugsnagUnity;
using BugsnagUnity.Payload;
using UnityEngine;

public class AndroidOnSendCallback : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AddOnSendError((@event) =>
        {
            @event.ApiKey = "Custom ApiKey";
            // AppWithState
            var app = @event.App;
            app.BinaryArch = "Custom BinaryArch";
            app.BuildUuid = "Custom BuildUuid";
            app.CodeBundleId = "Custom CodeBundleId";
            app.Id = "Custom Id";
            app.ReleaseStage = "Custom ReleaseStage";
            app.Type = "Custom Type";
            app.Version = "Custom Version";
            app.VersionCode = 999;
            app.Duration = TimeSpan.FromMilliseconds(1000);
            app.DurationInForeground = TimeSpan.FromMilliseconds(2000);
            app.InForeground = false;
            app.IsLaunching = false;

            @event.Context = "Custom Context";

            // Device with state
            var device = @event.Device;
            device.Id = "Custom Device Id";
            device.Locale = "Custom Locale";
            device.Manufacturer = "Custom Manufacturer";
            device.Model = "Custom Model";
            device.OsName = "Custom OsName";
            device.OsVersion = "Custom OsVersion";
            device.TotalMemory = 555;
            device.Jailbroken = true;
            device.CpuAbi = new string[] { "poo", "baar" };
            device.Orientation = "Custom Orientation";
            device.Time = new DateTimeOffset(1985, 08, 21, 01, 01, 01, TimeSpan.Zero);

            // breadcrumbs
            foreach (var crumb in @event.Breadcrumbs)
            {
                crumb.Type = BreadcrumbType.User;
                crumb.Message = "Custom Message";
                crumb.Metadata = new Dictionary<string, object>() { { "Custom", "Metadata" } };
            }

            // Errors
            foreach (var error in @event.Errors)
            {
                error.ErrorClass = "Custom ErrorClass";
                error.ErrorMessage = "Custom ErrorMessage";
                foreach (var trace in error.Stacktrace)
                {
                    trace.Method = "Custom Method";
                    trace.File = "Custom File";
                    trace.InProject = false;
                    trace.LineNumber = 123123;
                }
            }

            @event.GroupingHash = "Custom GroupingHash";

            @event.Severity = Severity.Info;

            @event.Unhandled = false;

            @event.GroupingDiscriminator = "Custom GroupingDiscriminator";

            // Threads
            foreach (var thread in @event.Threads)
            {
                thread.Name = "Custom Name";
            }

            var testDict = new Dictionary<string, object>();
            testDict.Add("scoop", "dewoop");
            @event.Device.RuntimeVersions = testDict;

            @event.SetUser("1", "2", "3");

            @event.AddMetadata("test", testDict);
            @event.AddMetadata("test2", testDict);

            @event.ClearMetadata("test2");

            @event.AddMetadata("test", "scoop", "dewoop");

            @event.AddFeatureFlag("test", "variant");
            @event.AddFeatureFlag("deleteMe");
            @event.ClearFeatureFlag("deleteMe");

            return true;
        });
    }
}
