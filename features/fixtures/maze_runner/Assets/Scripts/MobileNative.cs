using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MobileNative : MonoBehaviour {

#if UNITY_IOS || UNITY_TVOS || UNITY_OSX

    [DllImport("__Internal")]
    private static extern void IosSignal();

    [DllImport("__Internal")]
    private static extern void TriggerCocoaCppException();

    [DllImport("__Internal")]
    private static extern void ClearPersistentData();

#endif

    public static void Crash()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.tvOS:
                IOSCrash();
                break;
            case RuntimePlatform.Android:
                TriggerJvmException();
                break;
            default:
                UnhandledCrash();
                break;
        }
    }




    private static void IOSCrash()
    {
#if UNITY_IOS || UNITY_TVOS || UNITY_OSX
        TriggerCocoaCppException();
#endif
    }

    public static void DoIosSignal()
    {
#if UNITY_IOS
        IosSignal();
#endif
    }

    public static void ClearIOSData()
    {
#if UNITY_IOS
        ClearPersistentData();
#endif
    }

    public static void TriggerJvmException()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass crashClass = new AndroidJavaClass("com.example.bugsnagcrashplugin.CrashHelper");
            crashClass.CallStatic("triggerJvmException");
        }
    }

    public static void TriggerBackgroundJavaCrash()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass crashClass = new AndroidJavaClass("com.example.bugsnagcrashplugin.CrashHelper");
            crashClass.CallStatic("triggerBackgroundJvmException");
        }
    }

    public static void RaiseNdkSignal()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
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
