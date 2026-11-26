using System;
using System.Linq;
using System.Runtime.CompilerServices;
using BugsnagUnity;

// Throws through System.Linq so the trace mixes app frames with framework frames.
// With the default configuration, System.* frames are not in-project and this class's frames are.
public class InProjectDefaults : Scenario
{
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
        throw new Exception("InProjectDefaults");
    }
}
