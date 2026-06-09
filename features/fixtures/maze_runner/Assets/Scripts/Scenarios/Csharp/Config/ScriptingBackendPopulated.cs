using BugsnagUnity;
using UnityEngine;

public class ScriptingBackendPopulated : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        Configuration = new Configuration(apiKey);
        Configuration.Endpoints = new EndpointConfiguration(host + "/notify", host + "/sessions");
        Configuration.AutoTrackSessions = false;
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Configuration.EnabledErrorTypes.OOMs = false;
        }
    }

    public override void Run()
    {
        DoSimpleNotify("ScriptingBackendPopulated");
    }
}
