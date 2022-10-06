using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioRunner : MonoBehaviour
{

    public void RunScenario(string scenarioName, string apiKey, string host)
    {
        var scenario = GetScenario(scenarioName);
        scenario.PrepareConfig(apiKey,host);
        scenario.StartBugsnag();
        scenario.Run();
    }

    private Scenario GetScenario(string scenarioName)
    {

        var scenarios = gameObject.GetComponentsInChildren<Scenario>();

        foreach (var scenario in scenarios)
        {
            if (scenario.GetType().Name.Equals(scenarioName))
            {
                return scenario;
            }
        }

        throw new System.Exception("Scenario not found: " + scenarioName);
    }

}
