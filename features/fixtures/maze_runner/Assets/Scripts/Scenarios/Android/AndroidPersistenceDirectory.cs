using System.IO;
using UnityEngine;

public class AndroidPersistenceDirectory : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.PersistenceDirectory = Application.persistentDataPath + "/myBugsnagCache";
    }

    public override void Run()
    {
        if (Directory.Exists(Application.persistentDataPath + "/myBugsnagCache"))
        {
            DoSimpleNotify("Directory Found");
        }
        else
        {
            DoSimpleNotify("Directory Not Found!");
        }
    }
}
