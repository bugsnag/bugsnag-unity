using System;
using BugsnagUnity;

public class SerialisationError : Scenario
{
    private class Break
    {
        public string BrokenString
        {
            get
            {
                throw new Exception("BrokenString");
                return string.Empty;
            }
        }
    }

    public override void Run()
    {
        Bugsnag.AddOnError(BrokenCallback);

        DoSimpleNotify("SerialisationErrorBroken"); //this should not get delivered

        Bugsnag.RemoveOnError(BrokenCallback);

        DoSimpleNotify("SerialisationError"); //this one should
    }

    private bool BrokenCallback(IEvent @event)
    {
        @event.AddMetadata("Test", "Test", new Break());
        return true;
    }
}

