using System.Collections.Generic;
using UnityEngine;
using BugsnagUnity;
using BugsnagUnity.Payload;
using System.Runtime.InteropServices;
using System;

public class Scenario : MonoBehaviour
{

#if UNITY_STANDALONE_OSX

    [DllImport("NativeCrashy")]
    private static extern void crashy_signal_runner(float num);

#endif

#if UNITY_IOS || UNITY_TVOS

    [DllImport("__Internal")]
    private static extern void RaiseCocoaSignal();

    [DllImport("__Internal")]
    private static extern void TriggerCocoaCppException();

#endif

    public const string FAIL_URL = "https://localhost:994";

    public Configuration Configuration;

    [HideInInspector]
    public string CustomStacktrace = "Main.CUSTOM1 () (at Assets/Scripts/Main.cs:123)\nMain.CUSTOM2 () (at Assets/Scripts/Main.cs:123)";

    public virtual void PrepareConfig(string apiKey, string host)
    {
        Configuration = new Configuration(apiKey);
        Configuration.Endpoints = new EndpointConfiguration(host + "/notify", host + "/sessions");
        Configuration.ScriptingBackend = FindScriptingBackend();
        Configuration.DotnetScriptingRuntime = FindDotnetScriptingRuntime();
        Configuration.DotnetApiCompatibility = FindDotnetApiCompatibility();
        Configuration.AutoTrackSessions = false;
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Configuration.EnabledErrorTypes.OOMs = false;
        }
    }

    public void AddSwitchConfigValues(SwitchCacheType switchCacheType, int switchCacheIndex, string switchMountName)
    {
        Configuration.SwitchCacheType = switchCacheType;
        Configuration.SwitchCacheIndex = switchCacheIndex;
        Configuration.SwitchCacheMountName = switchMountName;
    }

    public virtual void StartBugsnag()
    {
        Bugsnag.Start(Configuration);
    }

    public virtual void Run()
    {

    }

    private static string FindScriptingBackend()
    {
#if ENABLE_MONO
        return "Mono";
#elif ENABLE_IL2CPP
      return "IL2CPP";
#else
      return "Unknown";
#endif
    }

    private static string FindDotnetScriptingRuntime()
    {
#if NET_4_6
        return ".NET 4.6 equivalent";
#else
        return ".NET 3.5 equivalent";
#endif
    }

    private static string FindDotnetApiCompatibility()
    {
#if NET_2_0_SUBSET
        return ".NET 2.0 Subset";
#else
        return ".NET 2.0";
#endif
    }

    public void AddTestingMetadata()
    {

        Bugsnag.AddMetadata("custom", new Dictionary<string, object>(){
            {"int", 123 },
            {"float", 123.123f },
          //  {"long", 12345678901234567890 }, pending PLAT-9426
            {"double", 123.456 },
            {"stringArray", new []{"1",null,"3"} },
            {"emptyStringArray", new string[]{} },
            {"intList", new List<int>(){1,2,3} },
            {"intArray", new []{4,5,6} },
            {"stringDict", new Dictionary<string,string>(){ {"hello","goodbye"} } }
        });

        Bugsnag.AddMetadata("clearMe", new Dictionary<string, object>(){
            {"test", "test" },
        });

        Bugsnag.ClearMetadata("clearMe");

        Bugsnag.AddMetadata("test", "test1", "test1");
        Bugsnag.AddMetadata("test", "test1", "test2");
        Bugsnag.AddMetadata("test", "nullMe", "notNull");
        Bugsnag.AddMetadata("test", "nullMe", null);

        Bugsnag.AddMetadata("app", new Dictionary<string, object>(){
            {"extra", "inApp" }
        });

        Bugsnag.AddMetadata("device", new Dictionary<string, object>(){
            {"extra", "inDevice" }
        });

    }

    public void AddTestingFeatureFlags()
    {
        Bugsnag.AddFeatureFlag("flag1","variant1");
        Bugsnag.AddFeatureFlag("flag2", "variant2");
        Bugsnag.AddFeatureFlag("flag3", "variant3");
        Bugsnag.ClearFeatureFlag("flag2");
    }

    public bool SimpleEventCallback(IEvent @event)
    {
        EditAllAppData(@event);
        EditAllDeviceData(@event);

        @event.Errors[0].ErrorClass = "ErrorClass";

        @event.Errors[0].Stacktrace[0].Method = "Method";

        @event.Errors[0].Stacktrace[0].LineNumber = 22;

        foreach (var crumb in @event.Breadcrumbs)
        {
            crumb.Message = "Custom Message";
            crumb.Type = BreadcrumbType.Request;
            crumb.Metadata = new Dictionary<string, object> { { "test", "test" } };
        }

        @event.AddMetadata("test1", new Dictionary<string, object> { { "test", "test" } });
        @event.AddMetadata("test2", new Dictionary<string, object> { { "test", "test" } });
        @event.ClearMetadata("test2");
        @event.AddFeatureFlag("fromCallback", "a");

        return true;
    }

    public bool SimpleSessionCallback(ISession session)
    {
        session.Id = "Custom Id";
        session.StartedAt = new DateTimeOffset(1985, 08, 21, 01, 01, 01, TimeSpan.Zero);
        session.SetUser("1","2","3");
        session.App.BinaryArch = "BinaryArch";
        session.App.BundleVersion = "BundleVersion";
        session.App.CodeBundleId = "CodeBundleId";
        session.App.DsymUuid = "DsymUuid";
        session.App.Id = "Id";
        session.App.ReleaseStage = "ReleaseStage";
        session.App.Type = "Type";
        session.App.Version = "Version";
        session.Device.Id = "Id";
        session.Device.Jailbroken = true;
        session.Device.Locale = "Locale";
        session.Device.Manufacturer = "Manufacturer";
        session.Device.Model = "Model";
        session.Device.OsName = "OsName";
        session.Device.OsVersion = "OsVersion";
        return true;
    }

    public bool CocoaNativeEventCallback(IEvent @event)
    {
        EditAllAppData(@event);
        EditAllDeviceData(@event);

        @event.Errors[0].ErrorClass = "ErrorClass";

        @event.Errors[0].Stacktrace[0].Method = "Method";

        //@event.Errors[0].Stacktrace[0].FrameAddress = "FrameAddress";

        @event.Errors[0].Stacktrace[0].IsLr = true;

        @event.Errors[0].Stacktrace[0].IsPc = true;

        @event.Errors[0].Stacktrace[0].MachoFile = "MachoFile";

        //@event.Errors[0].Stacktrace[0].MachoLoadAddress = "MachoLoadAddress";

        @event.Errors[0].Stacktrace[0].MachoUuid = "MachoUuid";

        //@event.Errors[0].Stacktrace[0].MachoVmAddress = "MachoVmAddress";

        //@event.Errors[0].Stacktrace[0].SymbolAddress = "SymbolAddress";


        foreach (var crumb in @event.Breadcrumbs)
        {
            crumb.Message = "Custom Message";
            crumb.Type = BreadcrumbType.Request;
            crumb.Metadata = new Dictionary<string, object> { { "test", "test" } };
        }

        @event.AddMetadata("test1", new Dictionary<string, object> { { "test", "test" } });
        @event.AddMetadata("test2", new Dictionary<string, object> { { "test", "test" } });
        @event.ClearMetadata("test2");

        @event.AddFeatureFlag("fromCallback", "a");

        @event.SetUser("4", "5", "6");

        return true;
    }

    private void EditAllAppData(IEvent @event)
    {

        // the unused vars for each property and there to check that the stored payload (retrieved from the native layers) contains the correct types
        // if the incorrect type is stored then we will get an InvalidCast Exception

        string binaryArch = @event.App.BinaryArch;
        @event.App.BinaryArch = "BinaryArch";

        string bundleVersion = @event.App.BundleVersion;
        @event.App.BundleVersion = "BundleVersion";

        string codeBundleId = @event.App.CodeBundleId;
        @event.App.CodeBundleId = "CodeBundleId";

        string dsymUuid = @event.App.DsymUuid;
        @event.App.DsymUuid = "DsymUuid";

        string id = @event.App.Id;
        @event.App.Id = "Id";

        string releaseStage = @event.App.ReleaseStage;
        @event.App.ReleaseStage = "ReleaseStage";

        string type = @event.App.Type;
        @event.App.Type = "Type";

        string version = @event.App.Version;
        @event.App.Version = "Version";

        bool? inForeground = @event.App.InForeground;
        @event.App.InForeground = false;

        bool? isLaunching = @event.App.IsLaunching;
        @event.App.IsLaunching = false;
    }

    private void EditAllDeviceData(IEvent @event)
    {
        string version = @event.Device.Id;
        @event.Device.Id = "Id";

        bool? jailBroken = @event.Device.Jailbroken;
        @event.Device.Jailbroken = true;

        string locale = @event.Device.Locale;
        @event.Device.Locale = "Locale";

        string manufacturer = @event.Device.Manufacturer;
        @event.Device.Manufacturer = "Manufacturer";

        string model = @event.Device.Model;
        @event.Device.Model = "Model";

        string osName = @event.Device.OsName;
        @event.Device.OsName = "OsName";

        string osVersion = @event.Device.OsVersion;
        @event.Device.OsVersion = "OsVersion";

        long? freeDisk = @event.Device.FreeDisk;
        @event.Device.FreeDisk = 123;

        long? freeMemory = @event.Device.FreeMemory;
        @event.Device.FreeMemory = 456;

        string orientation = @event.Device.Orientation;
        @event.Device.Orientation = "Orientation";
    }

    public void DoSimpleNotify(string msg)
    {
        Bugsnag.Notify(new System.Exception(msg));
    }

    public void MacOSNativeCrash()
    {
#if UNITY_STANDALONE_OSX
        crashy_signal_runner(8);
#endif
    }

    public void JvmException()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass crashClass = new AndroidJavaClass("com.example.bugsnagcrashplugin.CrashHelper");
            crashClass.CallStatic("triggerJvmException");
        }
    }

    public void BackgroundJVMException()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass crashClass = new AndroidJavaClass("com.example.bugsnagcrashplugin.CrashHelper");
            crashClass.CallStatic("triggerBackgroundJvmException");
        }
    }

    public void NdkSignal()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass crashClass = new AndroidJavaClass("com.example.bugsnagcrashplugin.CrashHelper");
            crashClass.CallStatic("raiseNdkSignal");
        }
    }

    public void IosException()
    {
#if UNITY_IOS || UNITY_TVOS
        TriggerCocoaCppException();
#endif
    }

    public void IosSignal()
    {
#if UNITY_IOS || UNITY_TVOS
        RaiseCocoaSignal();
#endif
    }

}
