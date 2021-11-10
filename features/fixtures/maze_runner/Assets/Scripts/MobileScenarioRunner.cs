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
        {"24", "Mark Launch Complete" },
        {"25", "Check Last Run Info" },
        {"26", "Native Event Callback" },
        {"27", "Ios Signal" },





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
        config.Endpoints = new EndpointConfiguration("http://bs-local.com:9339/notify", "http://bs-local.com:9339/sessions");
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
                config.AutoDetectErrors = false;
                config.EnabledErrorTypes.NativeCrashes = false;
                config.EnabledErrorTypes.UnhandledExceptions = false;
                config.DiscardClasses = new string[] { "St13runtime_error" };
                break;
            case "Disable Native Errors":
                config.EnabledErrorTypes.NativeCrashes = false;
                config.DiscardClasses = new string[] { "St13runtime_error" };
                break;
            case "Log error":
                config.NotifyLogLevel = LogType.Error;
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
            case "Native Event Callback":
                config.AddOnSendError((@event) => {

                    @event.App.BinaryArch = "BinaryArch";
                    @event.App.BundleVersion = "BundleVersion";
                    @event.App.CodeBundleId = "CodeBundleId";
                    @event.App.DsymUuid = "DsymUuid";
                    @event.App.Id = "Id";
                    @event.App.ReleaseStage = "ReleaseStage";
                    @event.App.Type = "Type";
                    @event.App.Version = "Version";
                    @event.App.InForeground = false;
                    @event.App.IsLaunching = false;

                    @event.Device.Id = "Id";
                    @event.Device.Jailbroken = true;
                    @event.Device.Locale = "Locale";
                    @event.Device.Manufacturer = "Manufacturer";
                    @event.Device.Model = "Model";
                    @event.Device.OsName = "OsName";
                    @event.Device.OsVersion = "OsVersion";
                    @event.Device.FreeDisk = 123;
                    @event.Device.FreeMemory = 123;
                    @event.Device.Orientation = "Orientation";

                    @event.Errors[0].ErrorClass = "ErrorClass";

                    @event.Errors[0].Stacktrace[0].Method = "Method";

                    return true;
                });
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
            case "Ios Signal":
                MobileNative.DoIosSignal();
                break;
            case "Check Last Run Info":
                CheckLastRunInfo();
                break;
            case "Mark Launch Complete":
                Bugsnag.MarkLaunchCompleted();
                ThrowException();
                break;
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
            case "Native Event Callback":
                break;
            case "iOS Native Error":
            case "iOS Native Error No Threads":
            case "Discard Error Class":
                NativeException();
                break;
            case "Disable Native Errors":
                MobileNative.TriggerBackgroundJavaCrash();
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

    private void CheckLastRunInfo()
    {
        var info = Bugsnag.GetLastRunInfo();
        if (info.Crashed && info.CrashedDuringLaunch && info.ConsecutiveLaunchCrashes > 0)
        {
            Bugsnag.Notify(new System.Exception("Last Run Info Correct"));
        }
    }

    private void AddMetadataForRedaction()
    {
        Bugsnag.AddMetadata("User", new Dictionary<string, object>() {
                    {"test","test" },
                    { "password","password" }
                });
    }

    private void CheckForCustomAndroidDir()
    {
        if (Directory.Exists(Application.persistentDataPath + "/myBugsnagCache"))
        {
            throw new System.Exception("Directory Found");
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
        Bugsnag.Notify(new ExecutionEngineException("This one has a callback"), @event =>
        {
            @event.Context = "Callback Context";
            @event.AddMetadata("Callback", new Dictionary<string, object>()
            {
                {"region", "US"}
            });
            return true;
        });
    }

    public void SetUser()
    {
        Bugsnag.SetUser("mcpacman", "configureduser@example.com", "Geordi McPacman");
    }

    public void AddMetadata()
    {
        Bugsnag.AddMetadata("ConfigMetadata", new Dictionary<string, object>(){
          { "subsystem", "Player Mechanics" }
        });
    }

    public void AddCallbackMetadata()
    {
        Bugsnag.AddOnError(@event =>
        {
            @event.AddMetadata("CallbackMetadata", new Dictionary<string, object>(){
                { "subsystem", "Player Mechanics" }
            });
            return true;
        });
    }

    public void AddCallbackContext()
    {
        Bugsnag.AddOnError(@event =>
        {
            @event.Context = "BeforeNotify Context";
            return true;
        });
    }

    public void AddCallbackUser()
    {
        Bugsnag.AddOnError(@event =>
        {
            @event.SetUser("lunchfrey", "Lunchfrey Jones", "beforenotifyuser@example.com");
            return true;
        });
    }

    public void RemoveAllCallbacks()
    {
        throw new ExecutionEngineException("Hmm, doesn't seem to exist");
    }

    public void LeaveBreadcrumbString()
    {
        Bugsnag.LeaveBreadcrumb("String breadcrumb clicked");
    }

    public void LeaveBreadcrumbTuple()
    {
        Bugsnag.LeaveBreadcrumb(
          "Tuple breadcrumb clicked",
          new Dictionary<string, object>() { { "scene", "SomeVeryRealScene" } },
          BreadcrumbType.Navigation);
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene("OtherScene");
    }
}
