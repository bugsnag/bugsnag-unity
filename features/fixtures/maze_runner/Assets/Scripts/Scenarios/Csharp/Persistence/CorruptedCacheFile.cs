using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CorruptedCacheFile : Scenario
{
    public override void Run()
    {
        var dirPath = Application.persistentDataPath + "/Bugsnag/Events";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllText(dirPath + "/f04274f7-6f7d-448e-b62e-486cc019a708.event", "NOT JSON");
    }
}
