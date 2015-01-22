using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NotifyButtonScript : MonoBehaviour {
		
#if UNITY_IPHONE
	[DllImport ("__Internal")]
	private static extern void ExampleNativeCrash();

	[DllImport ("__Internal")]
	private static extern void ExampleCrashInBackground();
#endif

#if UNITY_EDITOR

	// Build a folder containing unity3d file and html file
	[MenuItem ("Build/BuildiOS")]
	public static void BuildIos(){
		string[] levels = {"Assets/Buttons.unity"};
		BuildPipeline.BuildPlayer(levels, "bugsnag-unity-ios", 
		                          BuildTarget.iPhone, BuildOptions.AutoRunPlayer); 
	}

	// Build a folder containing unity3d file and html file
	[MenuItem ("Build/Build Android")]
	public static void BuildAndroid(){
		string[] levels = {"Assets/Buttons.unity"};
		BuildPipeline.BuildPlayer(levels, "bugsnag-unity-android", 
		                          BuildTarget.Android, BuildOptions.AutoRunPlayer); 
	}
#endif

	// Use this for initialization
	void Start () {
		Bugsnag.NotifyUrl = "http://192.168.1.2:8000/";
		Bugsnag.Context = null;
		Bugsnag.UserId = null;
		Bugsnag.ReleaseStage = null;
		Bugsnag.AddToTab (null, null, null);
		Bugsnag.AddToTab ("this", "null", null);
		Bugsnag.ClearTab (null);
		Bugsnag.UserId = "four";
		Bugsnag.ReleaseStage = "staging";
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void OnStacktraceClick() {
		Debug.Log ("Stacktrace clicked >>");
		Debug.Log (new System.Diagnostics.StackTrace (true).ToString ());
		Debug.Log ("<<<");
	}

	public void OnSegfaultClick () {
		Debug.Log ("Segfault clicked");
		Marshal.ReadInt32 (System.IntPtr.Zero);
	}

	public void OnCrashClick () {
		Debug.Log ("Crash clicked");
		throw new System.Exception ("Crash clicked!");
	}

	public void OnNotifyClick () {
		Debug.Log ("Notify clicked");
		Bugsnag.Notify (new System.Exception ("Notify clicked!"));
	}

	public void OnDivideByZeroClick () {
		Debug.Log ("Divide By Zero clicked");
		try {
			int a = 0;
			int b = 1;
			int c = b / a;
		} catch (System.Exception e) {
			Bugsnag.Notify (e);
		}
	}

	public void OnLogDebugExceptionClick () {
		Debug.Log ("Log.DebugException clicked");
		Debug.LogException (new System.Exception ("Debug Exception clicked!"));
	}

	public void OnLogDebugErrorClick () {
		Debug.Log ("Log.DebugError clicked");
		Debug.LogError ("Debug Error clicked!");
	}
	
	public void OnNativeCrashClick () {
#if UNITY_IPHONE
		ExampleNativeCrash();
#else
		AndroidJavaObject jo = new AndroidJavaObject("ExampleCrash");
		jo.Call("Crash");
#endif
	}

	public void OnThreadCrashClick () {
#if UNITY_IPHONE
		ExampleCrashInBackground();
#else
		AndroidJavaObject jo = new AndroidJavaObject("ExampleCrash");
		jo.Call("CrashBackgroundThread");
#endif
	}
}
