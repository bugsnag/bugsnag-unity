using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class NotifyButtonScript : MonoBehaviour {
		
#if UNITY_IPHONE
	[DllImport ("__Internal")]
	private static extern void ExampleNativeCrash();

	[DllImport ("__Internal")]
	private static extern void ExampleCrashInBackground();
#endif
	
	// Use this for initialization
	void Start () {

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
		throw new System.Exception ("Crash bang wallop!");
	}

	public void OnNotifyClick () {
		Debug.Log ("Notify clicked");
		Bugsnag.Notify (new System.Exception ("Hello Bugsnag"));
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
		Debug.LogException (new System.Exception ("yoyo"));
	}

	public void OnLogDebugErrorClick () {
		Debug.Log ("Log.DebugError clicked");
		Debug.LogError ("zozo");
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
