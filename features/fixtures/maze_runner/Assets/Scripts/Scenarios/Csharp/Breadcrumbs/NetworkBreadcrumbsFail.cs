using System.Collections;
using BugsnagNetworking;
using UnityEngine;

public class NetworkBreadcrumbsFail: Scenario
{

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.EnabledBreadcrumbTypes = new BugsnagUnity.Payload.BreadcrumbType[] { BugsnagUnity.Payload.BreadcrumbType.Request };
    }

    public override void Run()
    {
       StartCoroutine(DoRun());
    }

    private IEnumerator DoRun()
    {
        var www = BugsnagUnityWebRequest.Get(FAIL_URL + "?success=false");
        yield return www.SendWebRequest();
        yield return new WaitForSeconds(1);
        throw new System.Exception("NetworkBreadcrumbsFail");
    }
}
