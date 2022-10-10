﻿
public class PersistSessionReport : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AddOnSession((session) =>
        {
            session.App.ReleaseStage = "Second Session";
            return true;
        });
    }
}
