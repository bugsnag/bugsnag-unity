public class MacOSNativeCrashEnabledErrorTypes : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.EnabledErrorTypes.Crashes = false;
    }

    public override void Run()
    {
        MacOSNativeCrash();
    }
}
