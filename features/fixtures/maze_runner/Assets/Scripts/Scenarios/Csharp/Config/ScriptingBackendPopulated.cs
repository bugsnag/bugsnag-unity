public class ScriptingBackendPopulated : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        // Deliberately do NOT set ScriptingBackend, DotnetScriptingRuntime, or
        // DotnetApiCompatibility — they should be auto-detected by Configuration.
        base.PrepareConfig(apiKey, host);
    }

    public override void Run()
    {
        DoSimpleNotify("ScriptingBackendPopulated");
    }
}
