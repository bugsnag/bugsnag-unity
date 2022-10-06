using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BugsnagUnity;

public class Scenario : MonoBehaviour
{


    public Configuration Configuration;

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
}
