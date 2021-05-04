using System;
using UnityEngine;
using BugsnagUnity;
using System.Collections.Generic;
using BugsnagUnity.Payload;
using UnityEngine.SceneManagement;

public class ReporterBehavior : MonoBehaviour {

    // Use this for initialization
    void Start () {
        Configuration config = new Configuration("12312312312312312312312312312312");
        config.Endpoint = new Uri("http://192.168.178.53:9339/notify");
        config.Endpoint = new Uri("http://192.168.178.53:9339/session");
        config.Context = "My context";
        config.AppVersion = "1.2.3";
        Bugsnag.Start(config);
    }

    public void ThrowException() {
        throw new System.Exception("You threw an exception!");
    }

    public void AssertionFailure() {
        var items = new int[]{1, 2, 3};
        Debug.Log("Item4 is: " + items[4]);
    }

    public void NativeException() {
        BugsnagNative.Crash();
    }

    public void LogCaughtException() {
        try {
            var items = new int[]{1, 2, 3};
            Debug.Log("Item4 is: " + items[4]);
        } catch (System.Exception ex) {
            Debug.LogException(ex);
        }
    }

    public void LogWithClasPrefix() {
        throw new ExecutionEngineException("Haven't gotten around to making this work, sorry");
    }

    public void NotifyCaughtException() {
        try {
            var items = new int[]{1, 2, 3};
            Debug.Log("Item4 is: " + items[4]);
        } catch (System.Exception ex) {
            Bugsnag.Notify(ex);
        }
    }
    public void NotifyWithCallback() {
        Bugsnag.Notify(new ExecutionEngineException("This one has a callback"), report =>
        {
            report.Context = "Callback Context";
            report.Metadata.Add("Callback", new Dictionary<string, string>()
            {
                {"region", "US"}
            });
        });
    }
    public void StartSession() {
        Bugsnag.SessionTracking.StartSession();
    }
    public void SetUser() {
        Bugsnag.User.Id = "mcpacman";
        Bugsnag.User.Name = "Geordi McPacman";
        Bugsnag.User.Email = "configureduser@example.com";
    }
    public void ClearUser() {
        Bugsnag.User.Clear();
    }
    public void AddMetadata() {
        Bugsnag.Metadata.Add("ConfigMetadata", new Dictionary<string, string>(){
          { "subsystem", "Player Mechanics" }
        });
    }
    public void AddCallbackMetadata() {
        Bugsnag.BeforeNotify(report =>
        {
            report.Metadata.Add("CallbackMetadata", new Dictionary<string, string>(){
                { "subsystem", "Player Mechanics" }
            });
        });
    }
    public void AddCallbackContext() {
        Bugsnag.BeforeNotify(report =>
        {
            report.Context = "BeforeNotify Context";
        });
    }
    public void AddCallbackUser() {
        Bugsnag.BeforeNotify(report =>
        {
            report.User.Id = "lunchfrey";
            report.User.Name = "Lunchfrey Jones";
            report.User.Email = "beforenotifyuser@example.com";
        });
    }
    public void AddCallbackSeverity() {
        Bugsnag.BeforeNotify(report =>
        {
            report.Severity = Severity.Info;
        });
    }
    public void AddCallbackCancellation() {
        Bugsnag.BeforeNotify(report =>
        {
            report.Ignore();
        });
    }
    public void RemoveAllCallbacks() {
        throw new ExecutionEngineException("Hmm, doesn't seem to exist");
    }
    public void LeaveBreadcrumbString() {
        Bugsnag.Breadcrumbs.Leave("String breadcrumb clicked");
    }
    public void LeaveBreadcrumbTuple() {
        Bugsnag.Breadcrumbs.Leave(
          "Tuple breadcrumb clicked",
          BreadcrumbType.Navigation,
          new Dictionary<string, string>() {{ "scene", "SomeVeryRealScene" }}
        );
    }
    public void ChangeScene() {
        SceneManager.LoadScene("OtherScene");
    }
}
