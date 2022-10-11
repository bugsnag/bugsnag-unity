using System.Collections.Generic;
using UnityEngine;
using BugsnagUnity;
using BugsnagUnity.Payload;

public class Scenario : MonoBehaviour
{


    public Configuration Configuration;

    [HideInInspector]
    public string CustomStacktrace = "Main.CUSTOM1 () (at Assets/Scripts/Main.cs:123)\nMain.CUSTOM2 () (at Assets/Scripts/Main.cs:123)";

    public virtual void PrepareConfig(string apiKey, string host)
    {
        Configuration = new Configuration(apiKey);
        Configuration.Endpoints = new EndpointConfiguration(host + "/notify", host + "/sessions");
        Configuration.ScriptingBackend = FindScriptingBackend();
        Configuration.DotnetScriptingRuntime = FindDotnetScriptingRuntime();
        Configuration.DotnetApiCompatibility = FindDotnetApiCompatibility();
    }

    public virtual void StartBugsnag()
    {
        Bugsnag.Start(Configuration);
    }

    public virtual void Run()
    {

    }

    private static string FindScriptingBackend()
    {
#if ENABLE_MONO
        return "Mono";
#elif ENABLE_IL2CPP
      return "IL2CPP";
#else
      return "Unknown";
#endif
    }

    private static string FindDotnetScriptingRuntime()
    {
#if NET_4_6
        return ".NET 4.6 equivalent";
#else
        return ".NET 3.5 equivalent";
#endif
    }

    private static string FindDotnetApiCompatibility()
    {
#if NET_2_0_SUBSET
        return ".NET 2.0 Subset";
#else
        return ".NET 2.0";
#endif
    }

    public void AddTestingMetadata()
    {
        Bugsnag.AddMetadata("init", new Dictionary<string, object>(){
            {"foo", "bar" },
        });
        Bugsnag.AddMetadata("test", "test1", "test1");
        Bugsnag.AddMetadata("test", "test2", "test2");
        Bugsnag.AddMetadata("custom", new Dictionary<string, object>(){
            {"letter", "QX" },
            {"better", 400 },
            {"string-array", new string []{"1","2","3"} },
            {"int-array", new int []{1,2,3} },
            {"dict", new Dictionary<string,object>(){ {"test" , 123 } } }
        });
        Bugsnag.AddMetadata("app", new Dictionary<string, object>(){
            {"buildno", "0.1" },
            {"cache", null },
        });
        Bugsnag.ClearMetadata("init");
        Bugsnag.ClearMetadata("test", "test2");
    }

    public void SetInvalidEndpoints()
    {
        Configuration.Endpoints = new EndpointConfiguration("https://notify.def-not-bugsnag.com", "https://notify.def-not-bugsnag.com");
    }

    public bool SimpleCallback(IEvent @event)
    {
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
        @event.Device.FreeMemory = 456;
        @event.Device.Orientation = "Orientation";

        @event.Errors[0].ErrorClass = "ErrorClass";

        @event.Errors[0].Stacktrace[0].Method = "Method";

        @event.Errors[0].Stacktrace[0].LineNumber = 22;

        foreach (var crumb in @event.Breadcrumbs)
        {
            crumb.Message = "Custom Message";
            crumb.Type = BreadcrumbType.Request;
            crumb.Metadata = new Dictionary<string, object> { { "test", "test" } };
        }

        @event.AddMetadata("test1", new Dictionary<string, object> { { "test", "test" } });
        @event.AddMetadata("test2", new Dictionary<string, object> { { "test", "test" } });
        @event.ClearMetadata("test2");

        @event.AddFeatureFlag("fromCallback", "a");

        return true;
    }
}
