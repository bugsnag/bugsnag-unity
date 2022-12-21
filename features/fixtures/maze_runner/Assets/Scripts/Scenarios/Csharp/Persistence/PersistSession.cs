
public class PersistSession : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AddOnSession((session) =>
        {
            session.App.ReleaseStage = "Session 1";
            return true;
        });
    }
}
