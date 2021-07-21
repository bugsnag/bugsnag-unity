using System;
using System.Collections;
using System.Collections.Generic;
using BugsnagUnity;
using BugsnagUnity.Payload;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MobileScenarioRunner : MonoBehaviour {

    private const float WAIT_TIME = 6;

    public InputField InputField;

    private Configuration GetMobileTestingConfig()
    {
        Configuration config = new Configuration("12312312312312312312312312312312");
        config.Endpoint = new Uri("http://bs-local.com:9339/notify");
        config.SessionEndpoint = new Uri("http://bs-local.com:9339/sessions");
        config.Context = "My context";
        config.AppVersion = "1.2.3";
        return config;
    }

    private void StartBugsnagAsNormal()
    {
        var config = GetMobileTestingConfig();
        config.NotifyLevel = LogType.Error;
        Bugsnag.Start(config);
        LeaveBreadcrumbString();
        LeaveBreadcrumbTuple();
    }

    public void TestDisabledBreadcrumbs()
    {
        var config = GetMobileTestingConfig();
        config.NotifyLevel = LogType.Exception;
        config.EnabledBreadcrumbTypes = new BreadcrumbType[0];
        Bugsnag.Start(config);
        throw new System.Exception("Disabled Breadcrumbs");
    }

    public void TestMaxBreadcrumbs()
    {
        var config = GetMobileTestingConfig();
        config.NotifyLevel = LogType.Exception;
        config.MaximumBreadcrumbs = 5;
        Bugsnag.Start(config);
        for (int i = 0; i < 5; i++)
        {
            Bugsnag.LeaveBreadcrumb("Crumb " + i);
        }
        throw new System.Exception("Max Breadcrumbs");
    }


    private IEnumerator WaitAndDo(Action action)
    {
        yield return new WaitForSeconds(WAIT_TIME);
        action.Invoke();
    }


    public void HandleScenarioInput()
    {

        var input = InputField.text;

        if (string.IsNullOrEmpty(input))
        {
            return;
        }

        switch (input)
        {
            case "throw Exception":
                TriggerThrowException();
                break;
            case "Log error":
                TriggerLogError();
                break;
            case "Native exception":
                TriggerNativeException();
                break;
            case "Log caught exception":
                TriggerLogCaughtException();
                break;
            case "NDK signal":
                TriggerNdkSignal();
                break;
            case "Notify caught exception":
                TriggerNotifyCaughtException();
                break;
            case "Notify with callback":
                TriggerNotifyWithCallback();
                break;
            case "Change scene":
                ChangeScene();
                break;
            case "Disable Breadcrumbs":
                TestDisabledBreadcrumbs();
                break;
            case "Start SDK":
                StartSDK();
                break;
            case "Max Breadcrumbs":
                StartSDK();
                break;
        }
    }



    public void StartSDK()
    {
        StartBugsnagAsNormal();
    }


    /// <summary>
    /// test throw an exception
    /// </summary>

    public void TriggerThrowException()
    {
        StartBugsnagAsNormal();
        StartCoroutine(WaitAndDo(ThrowException));
    }

    private void ThrowException()
    {
        throw new System.Exception("You threw an exception!");
    }



    /// <summary>
    /// test log an error
    /// </summary>


    public void TriggerLogError()
    {
        StartBugsnagAsNormal();
        SetUser();
        StartCoroutine(WaitAndDo(LogError));
    }

    private void LogError()
    {
        Debug.LogError("Something went wrong.");
    }

    /// <summary>
    /// test a native exception
    /// </summary>

    public void TriggerNativeException()
    {
        StartBugsnagAsNormal();
        StartCoroutine(WaitAndDo(NativeException));
    }

    private void NativeException()
    {
        MobileNative.Crash();
    }

    /// <summary>
    /// test log caught exception
    /// </summary>

    public void TriggerLogCaughtException()
    {
        StartBugsnagAsNormal();
        StartCoroutine(WaitAndDo(LogCaughtException));
    }

    private void LogCaughtException()
    {
        try
        {
            var items = new int[] { 1, 2, 3 };
            Debug.Log("Item4 is: " + items[4]);
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    /// <summary>
    /// test ndk signal
    /// </summary>

    public void TriggerNdkSignal()
    {
        StartBugsnagAsNormal();
        StartCoroutine(WaitAndDo(NdkSignal));
    }

    private void NdkSignal()
    {
        MobileNative.RaiseNdkSignal();
    }


    /// <summary>
    /// test notify caught exception
    /// </summary>

    public void TriggerNotifyCaughtException()
    {
        StartBugsnagAsNormal();
        StartCoroutine(WaitAndDo(NotifyCaughtException));
    }

    private void NotifyCaughtException()
    {
        try
        {
            var items = new int[] { 1, 2, 3 };
            Debug.Log("Item4 is: " + items[4]);
        }
        catch (System.Exception ex)
        {
            Bugsnag.Notify(ex);
        }
    }


    /// <summary>
    /// test notify with callback
    /// </summary>


    public void TriggerNotifyWithCallback()
    {
        StartBugsnagAsNormal();
        StartCoroutine(WaitAndDo(NotifyWithCallback));
    }

    public void NotifyWithCallback()
    {
        Bugsnag.Notify(new ExecutionEngineException("This one has a callback"), report =>
        {
            report.Context = "Callback Context";
            report.Metadata.Add("Callback", new Dictionary<string, string>()
            {
                {"region", "US"}
            });
        });
    }


    public void StartSession()
    {

        Bugsnag.SessionTracking.StartSession();
    }
    public void SetUser()
    {
        Bugsnag.User.Id = "mcpacman";
        Bugsnag.User.Name = "Geordi McPacman";
        Bugsnag.User.Email = "configureduser@example.com";
    }
    public void ClearUser()
    {
        Bugsnag.User.Clear();
    }
    public void AddMetadata()
    {
        Bugsnag.Metadata.Add("ConfigMetadata", new Dictionary<string, string>(){
          { "subsystem", "Player Mechanics" }
        });
    }
    public void AddCallbackMetadata()
    {
        Bugsnag.BeforeNotify(report =>
        {
            report.Metadata.Add("CallbackMetadata", new Dictionary<string, string>(){
                { "subsystem", "Player Mechanics" }
            });
        });
    }
    public void AddCallbackContext()
    {
        Bugsnag.BeforeNotify(report =>
        {
            report.Context = "BeforeNotify Context";
        });
    }
    public void AddCallbackUser()
    {
        Bugsnag.BeforeNotify(report =>
        {
            report.User.Id = "lunchfrey";
            report.User.Name = "Lunchfrey Jones";
            report.User.Email = "beforenotifyuser@example.com";
        });
    }
    public void AddCallbackSeverity()
    {
        Bugsnag.BeforeNotify(report =>
        {
            report.Severity = Severity.Info;
        });
    }
    public void AddCallbackCancellation()
    {
        Bugsnag.BeforeNotify(report =>
        {
            report.Ignore();
        });
    }
    public void RemoveAllCallbacks()
    {
        throw new ExecutionEngineException("Hmm, doesn't seem to exist");
    }
    public void LeaveBreadcrumbString()
    {
        Bugsnag.Breadcrumbs.Leave("String breadcrumb clicked");
    }
    public void LeaveBreadcrumbTuple()
    {
        Bugsnag.Breadcrumbs.Leave(
          "Tuple breadcrumb clicked",
          BreadcrumbType.Navigation,
          new Dictionary<string, string>() { { "scene", "SomeVeryRealScene" } }
        );
    }
    public void ChangeScene()
    {
        SceneManager.LoadScene("OtherScene");
    }
}
