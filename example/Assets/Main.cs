using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

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

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void RaiseCocoaSignal();

	[DllImport("__Internal")]
	private static extern void TriggerCocoaCppException();

	[DllImport("__Internal")]
	private static extern void TriggerCocoaOom();

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
		throw new Exception("Triggered an uncaught C# exception");
	}

	private void OnBugsnagNotifyClick()
	{
		BugsnagUnity.Bugsnag.Client.Notify(new Exception ("Sending a caught C# exception to Bugsnag"), report =>
		 	{
		 		report.Context = "NotifyClicked";
		 	});
	}

	private void OnLogExceptionClick()
	{
		Debug.LogException(new Exception("Sent an exception to Debug.LogException"));
	}
	

	private void OnLogErrorClick()
	{
		Debug.LogError(new Exception("Logged an error message in Debug.LogError"));
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
		Marshal.ReadInt32(IntPtr.Zero);
	}

	private void OnNativeSignalClick()
	{
	#if UNITY_IOS
		RaiseCocoaSignal();
	#elif UNITY_ANDROID
		using (var java = new AndroidJavaObject("com.example.lib.BugsnagCrash")) {
			java.Call("raiseNdkSignal");
		}
	#else
		WarnPlatformNotSupported();
	#endif
	}

	private void OnNativeCppExceptionClick()
	{
	#if UNITY_IOS
		TriggerCocoaCppException();
	#elif UNITY_ANDROID
		using (var java = new AndroidJavaObject("com.example.lib.BugsnagCrash")) {
			java.Call("throwCppException");
		}
	#else
		WarnPlatformNotSupported();
	#endif
	}

	private void OnJvmExceptionClick()
	{
		if (Application.platform == RuntimePlatform.Android) {
			using (var java = new AndroidJavaObject("com.example.lib.BugsnagCrash")) {
				java.Call("throwBackgroundJvmException");
			}
		} else {
			WarnPlatformNotSupported();
		}
	}

	private void OnApplicationNotRespondingClick()
	{
		if (Application.platform == RuntimePlatform.Android) {
			using (var java = new AndroidJavaObject("com.example.lib.BugsnagCrash")) {
				java.Call("triggerAnr");
			}
		} else {
			WarnPlatformNotSupported();
		}
	}

	private void OnOutOfMemoryClick()
	{
	#if UNITY_IOS
		TriggerCocoaOom();
	#else
		WarnPlatformNotSupported();
	#endif
	}
	private void OnAppHangClick()
	{
	#if UNITY_IOS
		TriggerCocoaAppHang();
	#else
		WarnPlatformNotSupported();
	#endif
	}

	private void WarnPlatformNotSupported() {
		Debug.Log("The current platform does not support triggering this type of error.");
	}
}
