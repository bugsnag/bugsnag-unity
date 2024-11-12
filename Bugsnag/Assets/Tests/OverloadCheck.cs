using System;
namespace BugsnagUnity.Tests
{
    public class OverloadCheck
    {

        // this method never runs and is just used to check that no notify overides are accidentally broken during refactoring
        // if one is broken then the notifier will not compile
        private void Check()
        {
            Bugsnag.Notify("name", "message", "stacktrace");

            Bugsnag.Notify("name", "message", "stacktrace", CallBack);

            Bugsnag.Notify(new Exception());

            Bugsnag.Notify(new Exception(), "stacktrace");

            Bugsnag.Notify(new Exception(), "stacktrace", CallBack);

            Bugsnag.Notify(new Exception(), CallBack);

            Bugsnag.Notify(new Exception(), Severity.Error);

            Bugsnag.Notify(new Exception(), Severity.Error, CallBack);
        }

        private bool CallBack(IEvent e)
        {
            return true;
        }
    }
}
