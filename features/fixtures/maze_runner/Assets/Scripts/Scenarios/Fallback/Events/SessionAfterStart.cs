using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionAfterStart : Scenario
{
    public override void Run()
    {
        if (Application.platform.Equals(RuntimePlatform.OSXPlayer))
        {
            Invoke("DoException",1);
        }
        else
        {
            DoException();
        }
    }

    private void DoException()
    {
        throw new System.Exception("SessionAfterStart");
    }
}
