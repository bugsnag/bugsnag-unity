using System.Collections;
using BugsnagNetworking;
using UnityEngine;

public class NetworkBreadcrumbsFail: Scenario
{


    public override void Run()
    {
       StartCoroutine(DoRun());
        Configuration.EnabledBreadcrumbTypes = new BugsnagUnity.Payload.BreadcrumbType[] { BugsnagUnity.Payload.BreadcrumbType.Request };
    }

    private IEnumerator DoRun()
    {
        var www = BugsnagUnityWebRequest.Get(FAIL_URL + "?success=false");
        yield return www.SendWebRequest();
        yield return new WaitForSeconds(1);
        throw new System.Exception("NetworkBreadcrumbsFail");
    }
}
