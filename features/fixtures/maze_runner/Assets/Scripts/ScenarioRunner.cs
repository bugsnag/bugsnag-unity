using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioRunner : MonoBehaviour
{

    public void RunScenario(string scenarioName, BasicConfigData basicConfigData)
    {
        var scenario = GetScenario(scenarioName);
        scenario.PreapareConfig(basicConfigData);
    }

    private Scenario GetScenario(string scenarioName)
    {

        var scenarios = gameObject.GetComponents<Scenario>();

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
