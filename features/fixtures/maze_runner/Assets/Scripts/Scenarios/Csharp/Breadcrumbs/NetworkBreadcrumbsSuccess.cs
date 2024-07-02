using System.Collections;
using BugsnagNetworking;
using UnityEngine;

public class NetworkBreadcrumbsSuccess : Scenario
{

    public override void Run()
    {
       StartCoroutine(DoRun());
    }

    private IEnumerator DoRun()
    {
        var get = BugsnagUnityWebRequest.Get(Main.MazeHost + "?success=true");
        yield return get.SendWebRequest();
        yield return new WaitForSeconds(1);

        var post = BugsnagUnityWebRequest.Post(Main.MazeHost, "{\"action\":\"success\"}");
        yield return post.SendWebRequest();
        yield return new WaitForSeconds(1);

        throw new System.Exception("NetworkBreadcrumbsSuccess");
    }
}
