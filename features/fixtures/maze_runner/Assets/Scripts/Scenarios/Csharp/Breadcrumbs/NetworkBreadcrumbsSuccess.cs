using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BugsnagNetworking;
using BugsnagUnity;
using UnityEngine;

public class NetworkBreadcrumbsSuccess : Scenario
{

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.RedactedKeys = new List<Regex> { new Regex("redactthis") };
        Configuration.EnabledBreadcrumbTypes = new BugsnagUnity.Payload.BreadcrumbType[] { BugsnagUnity.Payload.BreadcrumbType.Request };
    }
    public override void Run()
    {
       StartCoroutine(DoRun());
    }

    private IEnumerator DoRun()
    {
        var get = BugsnagUnityWebRequest.Get(Main.MazeHost + "?success=true&redactthis=notRedacted");
        yield return get.SendWebRequest();
        yield return new WaitForSeconds(1);

        var post = BugsnagUnityWebRequest.Post(Main.MazeHost, "{\"action\":\"success\"}");
        yield return post.SendWebRequest();
        yield return new WaitForSeconds(1);

        throw new System.Exception("NetworkBreadcrumbsSuccess");
    }
}
