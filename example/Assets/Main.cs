using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
		LogDebug,
		LogDebugWarning,
		LogDebugError,
		LogDebugException,
		CSHException,
		Notify,
		OOM,
		AppHang,
		CPPException,
		Signal,
		NullDereference;

	void Start ()
	{
		LogDebug.GetComponent<Button>().onClick.AddListener(OnLogDebugClick);
		LogDebugWarning.GetComponent<Button>().onClick.AddListener(OnLogDebugWarningClick);
		LogDebugError.GetComponent<Button>().onClick.AddListener(OnLogDebugErrorClick);
		LogDebugException.GetComponent<Button>().onClick.AddListener(OnLogDebugExceptionClick);
		CSHException.GetComponent<Button>().onClick.AddListener(OnCSHExceptionClick);
		Notify.GetComponent<Button>().onClick.AddListener(OnNotifyClick);
		OOM.GetComponent<Button>().onClick.AddListener(OnOOMClick);
		AppHang.GetComponent<Button>().onClick.AddListener(OnAppHangClick);
		CPPException.GetComponent<Button>().onClick.AddListener(OnCPPExceptionClick);
		Signal.GetComponent<Button>().onClick.AddListener(OnSignalClick);
		NullDereference.GetComponent<Button>().onClick.AddListener(OnNullDereferenceClick);
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void TriggerCPPException();

	[DllImport("__Internal")]
	private static extern void TriggerSignal();
#endif

	private void OnLogDebugClick()
	{
		Debug.Log("LogDebug clicked");
	}

	private void OnLogDebugWarningClick()
	{
		Debug.Log("LogDebugWarning clicked");
		Debug.LogWarning(new Exception("LogDebugWarning clicked"));
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

	private void OnOOMClick()
	{
		Debug.Log("OOM clicked");
		var list = new List<object>();
		while(true)
		{
			list.Add(AllocLargeObject());
		}
	}

	private void OnAppHangClick()
	{
		Debug.Log("AppHang clicked");
        while(true)
        {
            Debug.Log("Forever loop");
        }
	}

	private void OnCPPExceptionClick()
	{
		Debug.Log("CPPException clicked");
#if UNITY_IOS
		TriggerCPPException();
#endif
	}

	private void OnSignalClick()
	{
		Debug.Log("Signal clicked");
#if UNITY_IOS
		TriggerSignal();
#endif
	}

    object AllocLargeObject() {
        var tmp = new System.Object[1024];
        for (int i = 0; i < 1024; i++)
            tmp[i] = new byte[1024*1024];

		return tmp;
    }

	private void OnCSHExceptionClick()
	{
		Debug.Log("CSH Exception clicked");
		throw new Exception("CSH Exeception clicked");
	}

	private void OnNotifyClick()
	{
		Debug.Log("Notify clicked");
		BugsnagUnity.Bugsnag.Client.Notify(new Exception("Notify clicked!"), report =>
		{
			report.Context = "NotifyClicked";
		});
	}

	private void OnNullDereferenceClick()
    {
		Debug.Log("Null Dereference clicked");
		object o = null;
		o.ToString();
	}

	void Update () {

	}
}
