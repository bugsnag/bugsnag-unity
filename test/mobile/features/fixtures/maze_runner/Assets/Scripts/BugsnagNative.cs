using System.Runtime.InteropServices;
using UnityEngine;

public class BugsnagNative {

    #if UNITY_IOS || UNITY_EDITOR_OSX || UNITY_TVOS || UNITY_OSX
    [DllImport ("__Internal")]
    private static extern void framework_crash_me();
    #endif

    public static void Crash()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.tvOS:
                BugsnagNative.IOSCrash();
                break;
            case RuntimePlatform.Android:
                BugsnagNative.TriggerJvmException();
                break;
            default:
                BugsnagNative.UnhandledCrash();
                break;
        }
    }

    private static void IOSCrash()
    {
        #if UNITY_IOS || UNITY_EDITOR_OSX || UNITY_TVOS || UNITY_OSX
        framework_crash_me();
        #endif
    }

    public static void TriggerJvmException()
    {
        if (Application.platform == RuntimePlatform.Android) {
            AndroidJavaClass crashClass = new AndroidJavaClass("com.example.bugsnagcrashplugin.CrashHelper");
            crashClass.CallStatic("triggerJvmException");
        }
    }

    public static void RaiseNdkSignal()
    {
        if (Application.platform == RuntimePlatform.Android) {
            AndroidJavaClass crashClass = new AndroidJavaClass("com.example.bugsnagcrashplugin.CrashHelper");
            crashClass.CallStatic("raiseNdkSignal");
        }
    }

    private static void UnhandledCrash()
    {
        Debug.Log("Generating UnhandledCrash");
        int x = 0;
        int y = 42;
        int z = y / x;
        Debug.Log(z.ToString());
    }
}
