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

	public Button
		Stacktrace,
		Segfault,
		ManagedCrash,
		Notify,
		DivideByZero,
		LogDebugException,
		LogDebugError,
		NativeCrash,
		NativeBackgroundCrash;

	void Start ()
	{
		Stacktrace.GetComponent<Button>().onClick.AddListener(OnStacktraceClick);
		Segfault.GetComponent<Button>().onClick.AddListener(OnSegfaultClick);
		ManagedCrash.GetComponent<Button>().onClick.AddListener(OnManagedCrashClick);
		Notify.GetComponent<Button>().onClick.AddListener(OnNotifyClick);
		DivideByZero.GetComponent<Button>().onClick.AddListener(OnDivideByZeroClick);
		LogDebugException.GetComponent<Button>().onClick.AddListener(OnLogDebugExceptionClick);
		LogDebugError.GetComponent<Button>().onClick.AddListener(OnLogDebugErrorClick);
		NativeCrash.GetComponent<Button>().onClick.AddListener(OnNativeCrashClick);
		NativeBackgroundCrash.GetComponent<Button>().onClick.AddListener(OnNativeBackgroundCrashClick);
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void Crash();

	[DllImport("__Internal")]
	private static extern void CrashInBackground();
#endif

	private void OnNativeCrashClick()
	{
#if UNITY_IOS
		Crash();
#elif UNITY_ANDROID
		using (var java = new AndroidJavaObject("BugsnagCrash"))
		{
			java.Call("Crash");
		}

#endif
	}

	private void OnNativeBackgroundCrashClick()
	{
#if UNITY_IOS
		CrashInBackground();
#elif UNITY_ANDROID
		using (var java = new AndroidJavaObject("BugsnagCrash"))
		{
			java.Call("BackgroundCrash");
		}

#endif
	}

	private void OnLogDebugErrorClick()
	{
		Debug.Log("LogDebugError clicked");
		Debug.LogError(new Exception("LogDebugError clicked"));
	}

	private void OnLogDebugExceptionClick()
	{
		Debug.Log("LogDebugException clicked");
		Debug.LogException(new Exception("LogDebugException clicked"));
	}

	private void OnDivideByZeroClick()
	{
		Debug.Log ("DivideByZero clicked");
		try {
			int a = 0;
			int b = 1;
			int c = b / a;
		} catch (Exception e) {
			BugsnagUnity.Bugsnag.Client.Notify(e);
		}
	}

	private void OnNotifyClick()
	{
		Debug.Log ("Notify clicked");
		 BugsnagUnity.Bugsnag.Client.Notify(new Exception ("Notify clicked!"), report =>
		 	{
		 		report.Context = "NotifyClicked";
		 	});
	}

	private void OnManagedCrashClick()
	{
		Debug.Log("ManagedCrash clicked");
		throw new Exception("ManagedCrash clicked");
	}

	private void OnSegfaultClick()
	{
		Debug.Log("Segfault clicked");
		Marshal.ReadInt32(IntPtr.Zero);
	}

	private void OnStacktraceClick()
	{
		Debug.Log("Stacktrace clicked");
		Debug.Log(new System.Diagnostics.StackTrace(true).ToString());
	}

	void Update () {

	}
}
