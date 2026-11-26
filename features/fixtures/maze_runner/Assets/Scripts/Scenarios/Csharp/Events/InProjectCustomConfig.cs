using System;
using System.Linq;
using System.Runtime.CompilerServices;
using BugsnagUnity;

// Same trace shape as InProjectDefaults, but the configuration flips the result:
// System.Linq is a project namespace (longer than the default "System" exclusion)
// and this class is excluded.
public class InProjectCustomConfig : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.ProjectNamespaces = new[] { "System.Linq" };
        Configuration.ExcludedNamespaces = Configuration.DefaultExcludedNamespaces
            .Concat(new[] { "InProjectCustomConfig" })
            .ToArray();
    }

    public override void Run()
    {
        try
        {
            ThrowThroughLinq();
        }
        catch (Exception e)
        {
            Bugsnag.Notify(e);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowThroughLinq()
    {
        new[] { 1 }.Select<int, int>(Explode).ToList();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int Explode(int value)
    {
        throw new Exception("InProjectCustomConfig");
    }
}
