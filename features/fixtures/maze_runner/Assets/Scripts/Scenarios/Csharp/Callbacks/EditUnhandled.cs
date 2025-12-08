using System;
using System.Collections;
using BugsnagUnity;
using UnityEngine;

public class EditUnhandled : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.AddOnError(SimpleEventCallback);
        Configuration.ReportExceptionLogsAsHandled = false;
        Configuration.AddOnError(OnErrorCallback);
        Configuration.AddOnSendError(OnSendCallback);
    }

    public override void Run()
    {
        Bugsnag.StartSession();
        StartCoroutine(DoTest());
    }

    private IEnumerator DoTest()
    {
        Bugsnag.Notify(new Exception("EditUnhandled Control"));
        yield return new WaitForSeconds(1);
        Bugsnag.Notify(new Exception("EditUnhandled HandledInNotifyCallback"), (report) =>
        {
            report.Unhandled = true;
            return true;
        });
        yield return new WaitForSeconds(1);
        Bugsnag.Notify(new Exception("EditUnhandled HandledInOnSendCallback"));
        yield return new WaitForSeconds(1);
        throw new Exception("EditUnhandled UnhandledInOnErrorCallback");
    }

    private bool OnErrorCallback(IEvent @event)
    {
        if (@event.Errors[0].ErrorMessage == "EditUnhandled UnhandledInOnErrorCallback")
        {
            @event.Unhandled = false;
            return true;
        }
        return true;
    }
    private bool OnSendCallback(IEvent @event)
    {
        if (@event.Errors[0].ErrorMessage == "EditUnhandled HandledInOnSendCallback")
        {
            @event.Unhandled = true;
            return true;
        }
        return true;
    }
}
