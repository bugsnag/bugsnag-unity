using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using BugsnagUnity;
using System.Collections.Generic;
using BugsnagUnity.Payload;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Main : MonoBehaviour
{
#if UNITY_EDITOR
	[MenuItem("Build/Build iOS")]
	public static void iOSBuild()
	{
			BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
			buildPlayerOptions.scenes = new[] { "Assets/Main.unity" };
			buildPlayerOptions.locationPathName = "example.ios";
			buildPlayerOptions.target = BuildTarget.iOS;
			buildPlayerOptions.options = BuildOptions.None;
			BuildPipeline.BuildPlayer(buildPlayerOptions);
	}

	[MenuItem("Build/Build Android")]
	public static void AndroidBuild()
	{
			BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
			buildPlayerOptions.scenes = new[] { "Assets/Main.unity" };
			buildPlayerOptions.locationPathName = "example.apk";
			buildPlayerOptions.target = BuildTarget.Android;
			buildPlayerOptions.options = BuildOptions.None;
			BuildPipeline.BuildPlayer(buildPlayerOptions);
	}
#endif

#if UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS
	[DllImport("__Internal")]
	private static extern void RaiseCocoaSignal();

	[DllImport("__Internal")]
	private static extern void TriggerCocoaCppException();

	[DllImport("__Internal")]
	private static extern void TriggerCocoaAppHang();
#endif

	public Button
		ManagedCrash,
		BugsnagNotify,
		LogException,
		LogError,
		LogWarning,
		Log,
		Segfault,
		NativeSignal,
		NativeCppException,
		JvmException,
		ApplicationNotResponding,
		OutOfMemory,
		AppHang;

	void Start()
	{
		ManagedCrash.GetComponent<Button>().onClick.AddListener(OnManagedCrashClick);
		BugsnagNotify.GetComponent<Button>().onClick.AddListener(OnBugsnagNotifyClick);
		LogException.GetComponent<Button>().onClick.AddListener(OnLogExceptionClick);
		LogError.GetComponent<Button>().onClick.AddListener(OnLogErrorClick);
		LogWarning.GetComponent<Button>().onClick.AddListener(OnLogWarningClick);
		Log.GetComponent<Button>().onClick.AddListener(OnLogClick);
		Segfault.GetComponent<Button>().onClick.AddListener(OnSegfaultClick);
		NativeSignal.GetComponent<Button>().onClick.AddListener(OnNativeSignalClick);
		NativeCppException.GetComponent<Button>().onClick.AddListener(OnNativeCppExceptionClick);
		JvmException.GetComponent<Button>().onClick.AddListener(OnJvmExceptionClick);
		ApplicationNotResponding.GetComponent<Button>().onClick.AddListener(OnApplicationNotRespondingClick);
		OutOfMemory.GetComponent<Button>().onClick.AddListener(OnOutOfMemoryClick);
		AppHang.GetComponent<Button>().onClick.AddListener(OnAppHangClick);
	}

	private void OnManagedCrashClick()
	{
		Debug.Log("OnManagedCrashClicked");

		throw new System.Exception("Triggered an uncaught C# exception");
	}

	private void OnBugsnagNotifyClick()
	{
		Debug.Log("OnBugsnagNotifyClicked");

		Bugsnag.Notify(new System.Exception ("Sending a caught C# exception to Bugsnag"), report =>
		{
		 	report.Context = "NotifyClicked";
			return true;
		});
	}

	private void OnLogExceptionClick()
	{
		Debug.LogException(new System.Exception("Sent an exception to Debug.LogException"));
	}
	

	private void OnLogErrorClick()
	{
		Debug.LogError(new System.Exception("Logged an error message in Debug.LogError"));
	}

	private void OnLogWarningClick()
	{
		Debug.LogWarning("Logged a warning message in Debug.LogWarning");
	}
	private void OnLogClick()
	{
		Debug.Log("Logged a message in Debug.Log");
	}

	private void OnSegfaultClick()
	{
		Debug.Log("OnSegfaultClicked");

		Marshal.ReadInt32(IntPtr.Zero);
	}

	private void OnNativeSignalClick()
	{
		Debug.Log("OnNativeSignalClicked");

#if (UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS) && !UNITY_EDITOR
		RaiseCocoaSignal();
#elif UNITY_ANDROID && !UNITY_EDITOR
		using (var java = new AndroidJavaObject("com.example.lib.BugsnagCrash")) {
			java.Call("raiseNdkSignal");
		}
#else
		WarnPlatformNotSupported();
#endif
	}

	private void OnNativeCppExceptionClick()
	{
		Debug.Log("OnNativeCppExceptionClicked");

#if (UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS) && !UNITY_EDITOR
		TriggerCocoaCppException();
#elif UNITY_ANDROID && !UNITY_EDITOR
		using (var java = new AndroidJavaObject("com.example.lib.BugsnagCrash")) {
			java.Call("throwCppException");
		}
#else
		WarnPlatformNotSupported();
#endif
	}

	private void OnJvmExceptionClick()
	{
		Debug.Log("OnJvmExceptionClicked");
#if UNITY_ANDROID && !UNITY_EDITOR
		using (var java = new AndroidJavaObject("com.example.lib.BugsnagCrash")) {
			java.Call("throwBackgroundJvmException");
		}
#else
		WarnPlatformNotSupported();
#endif
	}

	private void OnApplicationNotRespondingClick()
	{
		Debug.Log("OnApplicationNotRespondingClicked");
#if UNITY_ANDROID && !UNITY_EDITOR
		using (var java = new AndroidJavaObject("com.example.lib.BugsnagCrash")) {
			java.Call("triggerAnr");
		}
#else
		WarnPlatformNotSupported();
#endif
	
	}

	private void OnOutOfMemoryClick()
	{
		Debug.Log("OnOutOfMemoryClicked");

#if (UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS) && !UNITY_EDITOR
		var list = new List<object>();
		while (true)
		{
			list.Add(AllocLargeObject());
		}
#else
		WarnPlatformNotSupported();
#endif
	}

	private object AllocLargeObject()
	{
		var tmp = new System.Object[1024];
		for (int i = 0; i < 1024; i++)
			tmp[i] = new byte[1024 * 1024];
		return tmp;
	}

	private void OnAppHangClick()
	{
		Debug.Log("OnAppHangClicked");

#if (UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS) && !UNITY_EDITOR
		TriggerCocoaAppHang();
#else
		WarnPlatformNotSupported();
#endif

	}

	private void WarnPlatformNotSupported() {
#if UNITY_EDITOR
		Debug.Log("This kind of error cannot be triggered in the Unity Editor.");
#else
		Debug.Log("The current platform does not support triggering this type of error.");
#endif
	}
}
