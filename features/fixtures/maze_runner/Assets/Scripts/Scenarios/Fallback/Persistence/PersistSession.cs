
public class PersistSession : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetInvalidEndpoints();
        Configuration.AddOnSession((session)=>
        {
            session.App.ReleaseStage = "First Session";
            return true;
        });
    }
}
