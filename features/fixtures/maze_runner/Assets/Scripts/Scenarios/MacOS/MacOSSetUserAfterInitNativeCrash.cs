using System.Collections;
using BugsnagUnity;
using UnityEngine;

public class MacOSSetUserAfterInitNativeCrash : Scenario
{
    public override void Run()
    {
        StartCoroutine(DoTest());
    }

    private IEnumerator DoTest()
    {
        Bugsnag.SetUser("1","2","3");
        yield return new WaitForSeconds(1);
        MacOSNativeCrash();
    }
}
