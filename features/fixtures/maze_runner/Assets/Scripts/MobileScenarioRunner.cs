using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BugsnagUnity;
using BugsnagUnity.Payload;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MobileScenarioRunner : MonoBehaviour {

    public Text Dialled, ScenarioName;

    private Dictionary<String, String> LOOKUP = new Dictionary<String, String>
    {
        // Scenarios
        {"01", "throw Exception" },
        {"02", "Log error" },
        {"03", "Native exception" },
        {"04", "Log caught exception" },
        {"05", "NDK signal" },
        {"06", "Notify caught exception" },
        {"07", "Notify with callback" },
        {"08", "Change scene" },
        {"09", "Disable Breadcrumbs" },
        {"10", "Start SDK" },
        {"11", "Max Breadcrumbs" },
        {"12", "Disable Native Errors" },
        {"13", "throw Exception with breadcrumbs" },
        {"14", "Start SDK no errors" },
        {"15", "Discard Error Class" },
        {"16", "Java Background Crash" },
        {"17", "Custom App Type" },
        {"18", "Android Persistence Directory" },
        {"19", "Disabled Release Stage" },
        {"20", "Enabled Release Stage" },
        {"21", "Java Background Crash No Threads" },
        {"22", "iOS Native Error" },
        {"23", "iOS Native Error No Threads" },





        // Commands
        {"90", "Clear iOS Data" },
    };

    private string GetNameFromDialCode(string code)
    {
        var scenarioName = LOOKUP[code];
        if (string.IsNullOrEmpty(scenarioName))
        {
            throw new System.Exception("Unable to find Scenario name for code: " + code);
        }
        return scenarioName;
    }

    private Configuration GetDefaultConfig()
    {
        Configuration config = new Configuration("12312312312312312312312312312312");
        config.Endpoint = new Uri("http://bs-local.com:9339/notify");
        config.SessionEndpoint = new Uri("http://bs-local.com:9339/sessions");
        config.Context = "My context";
        config.AppVersion = "1.2.3";
        config.BundleVersion = "1.2.3";
        config.RedactedKeys = new string[] { "test", "password" };
        config.VersionCode = 123;
        return config;
    }

    public void Dial(string number)
    {
        Dialled.text += number;
    }

    // Issues a command to the test fixture
    public void RunCommand()
    {
        // 1: Get the dial code
        var code = Dialled.text;
        Debug.Log("RunCommand called, code is " + code);
        if (string.IsNullOrEmpty(code) || code.Length != 2)
        {
            throw new System.Exception("Code is empty or not correctly formatted: " + code);
        }

        // 2: Get the command name and clear the number
        var scenarioName = GetNameFromDialCode(code);
        ScenarioName.text = scenarioName;
        Dialled.text = string.Empty;

        // 3: Issue the command
        DoTestAction(scenarioName);
    }

    // Tells the test fixture to run a particular test scenario
    public void RunScenario()
    {
        // 1: Get the dial code
        var code = Dialled.text;
        Debug.Log("RunScenario called, code is " + code);
        if (string.IsNullOrEmpty(code) || code.Length != 2)
        {
            throw new System.Exception("Code is empty or not correctly formatted: " + code);
        }

        // 2: Get the scenario name and clear the number
        var scenarioName = GetNameFromDialCode(code);
        ScenarioName.text = scenarioName;
        Dialled.text = string.Empty;

        // 3: Get the config for that scenario
        var config = PreapareConfigForScenario(scenarioName);

        // 4: Start the Bugsnag SDK
        StartTheSdk(config);

        // 5: Trigger the actions for the test
        DoTestAction(scenarioName);
    }

    private Configuration PreapareConfigForScenario(string scenarioName)
    {
        var config = GetDefaultConfig();

        switch (scenarioName)
        {
            case "Android Persistence Directory":
                config.PersistenceDirectory = Application.persistentDataPath + "/myBugsnagCache";
                break;
            case "Disabled Release Stage":
                config.EnabledReleaseStages = new string[] { "test" };
                config.ReleaseStage = "somevalue";
                break;
            case "Enabled Release Stage":
                config.EnabledReleaseStages = new string[] { "test" };
                config.ReleaseStage = "test";
                break;
            case "Java Background Crash":
            case "Native exception":
                config.ProjectPackages = new string[] { "test.test.test" };
                break;
            case "iOS Native Error No Threads":
            case "Java Background Crash No Threads":
                config.SendThreads = ThreadSendPolicy.NEVER;
                break;
            case "Start SDK no errors":
            case "Disable Native Errors":
                config.EnabledErrorTypes = new ErrorTypes[0];
                config.DiscardClasses = new string[] { "St13runtime_error" };
                break;
            case "Log error":
                config.NotifyLevel = LogType.Error;
                break;
            case "Disable Breadcrumbs":
                config.EnabledBreadcrumbTypes = new BreadcrumbType[0];
                break;
            case "Max Breadcrumbs":
                config.MaximumBreadcrumbs = 5;
                break;
            case "NDK signal":
                break;
            case "Custom App Type":
                config.AppType = "test";
                break;
            case "Discard Error Class":
#if UNITY_IOS
                config.DiscardClasses = new string[] { "St13runtime_error" };

#elif UNITY_ANDROID
                config.DiscardClasses = new string[] { "java.lang.ArrayIndexOutOfBoundsException" };
#endif
                break;
        }
        return config;
    }

    private void StartTheSdk(Configuration config)
    {
        Bugsnag.Start(config);
    }

    public void DoTestAction(string scenarioName)
    {
        switch (scenarioName)
        {
            case "Android Persistence Directory":
                CheckForCustomAndroidDir();
                break;
            case "Disabled Release Stage":
            case "Enabled Release Stage":
#if UNITY_ANDROID
                MobileNative.TriggerBackgroundJavaCrash();
#elif UNITY_IOS
                NativeException();
#endif  
                break;
            case "Custom App Type":
                ThrowException();
                break;
            case "Start SDK":
            case "Start SDK no errors":
                break;
            case "iOS Native Error":
            case "iOS Native Error No Threads":
            case "Discard Error Class":
            case "Disable Native Errors":
                NativeException();
                break;
            case "throw Exception":
                ThrowException();
                break;
            case "throw Exception with breadcrumbs":
                AddMetadataForRedaction();
                LeaveBreadcrumbString();
                LeaveBreadcrumbTuple();
                ThrowException();
                break;
            case "Log error":
                SetUser();
                LogError();
                break;
            case "Java Background Crash No Threads":
            case "Java Background Crash":
                AddMetadataForRedaction();
                MobileNative.TriggerBackgroundJavaCrash();
                break;
            case "Native exception":
                LeaveBreadcrumbString();
                LeaveBreadcrumbTuple();
                NativeException();
                break;
            case "Log caught exception":
                LeaveBreadcrumbString();
                LeaveBreadcrumbTuple();
                LogCaughtException();
                break;
            case "NDK signal":
                NdkSignal();
                break;
            case "Notify caught exception":
                NotifyCaughtException();
                break;
            case "Notify with callback":
                LeaveBreadcrumbString();
                LeaveBreadcrumbTuple();
                // Wait 6 seconds for launch timer to be over
                Invoke("NotifyWithCallback" , 6);
                break;
            case "Change scene":
                ChangeScene();
                break;
            case "Disable Breadcrumbs":
                ThrowException();
                break;
            case "Max Breadcrumbs":
                LeaveFiveBreadcrumbs();
                ThrowException();
                break;
            case "Clear iOS Data":
                MobileNative.ClearIOSData();
                break;
            default:
                throw new System.Exception("Unknown scenario: " + scenarioName);
        }
    }

    private void AddMetadataForRedaction()
    {
        Bugsnag.Metadata.Add("User", new Dictionary<string, string>() {
                    {"test","test" },
                    { "password","password" }
                });
    }

    private void CheckForCustomAndroidDir()
    {
        if (Directory.Exists(Application.persistentDataPath + "/myBugsnagCache"))
        {
            ThrowException();
        }
    }

    public void LeaveFiveBreadcrumbs()
    {
        for (int i = 0; i < 5; i++)
        {
            Bugsnag.LeaveBreadcrumb("Crumb " + i);
        }
    }

    private void ThrowException()
    {
        throw new System.Exception("You threw an exception!");
    }

    private void LogError()
    {
        Debug.LogError("Something went wrong.");
    }

    private void NativeException()
    {
        MobileNative.Crash();
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

    private void NdkSignal()
    {
        MobileNative.RaiseNdkSignal();
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
