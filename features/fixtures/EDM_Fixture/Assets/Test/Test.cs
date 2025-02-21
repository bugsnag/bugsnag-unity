using System.Collections;
using System.Collections.Generic;
using BugsnagUnity;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Bugsnag.Start(GetDefaultConfig());
        throw new System.Exception("EDM4U");
    }

    private Configuration GetDefaultConfig()
    {
        Configuration config = new Configuration("12312312312312312312312312312312");
        config.Endpoints = new EndpointConfiguration("http://bs-local.com:9339/notify", "http://bs-local.com:9339/sessions");
        return config;
    }
   
}
