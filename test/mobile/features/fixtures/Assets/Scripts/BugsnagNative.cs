using System.Collections;
using System.Collections.Generic;
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
                BugsnagNative.IOSCrash ();
                break;
            case RuntimePlatform.Android:
                BugsnagNative.AndroidCrash ();
                break;
            default:
                BugsnagNative.UnhandledCrash ();
                break;
        }
    }

    private static void IOSCrash()
    {
        #if UNITY_IOS || UNITY_EDITOR_OSX || UNITY_TVOS || UNITY_OSX
        framework_crash_me();
        #endif
    }

    private static void AndroidCrash()
    {
        AndroidJavaClass crashClass = new AndroidJavaClass ("com.example.bugsnagcrashplugin.CrashHelper");
        crashClass.CallStatic ("UnhandledCrash");
    }

    private static void UnhandledCrash()
    {
        Debug.Log ("Generating UnhandledCrash");
        int x = 0;
        int y = 42;
        int z = y / x;
        Debug.Log (z.ToString ());
    }
}
