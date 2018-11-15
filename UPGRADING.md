Upgrading
=========

## 3.x to 4.x

*Our Unity notifier has gone through some major improvements, and there are some changes you'll need to make to get onto the new version.*

### Remove old Bugsnag files from your Unity project

- Remove the `Assets/Plugins/iOS/Bugsnag` directory
- Remove the `Assets/Plugins/OSX/Bugsnag` directory
- Remove `Assets/Plugins/Android/bugsnag-android-release.aar`
- Remove `Assets/Plugins/Android/bugsnag-android-unity-release.aar`
- Remove `Assets/Plugins/Android/sdk-release.aar`
- Remove `Assets/Plugins/WebGL/bugsnag.jspre`
- Remove `Assets/Plugins/WebGL/BugsnagAppTimings.jspre`
- Remove `Assets/Plugins/WebGL/BugsnagUnity.jslib`
- Remove `Assets/Standard Assets/Bugsnag/Bugsnag.cs`
- Remove `Assets/Standard Assets/Bugsnag/Editor/BugsnagPostProcess.cs`

### Import the new package

- Download version 4.x.x `Bugsnag.unitypackage` from the [Releases pages](https://github.com/bugsnag/bugsnag-unity/releases)
- Import into your Unity project

### Relink the MonoBehaviour

If you used the basic configuration to configure Bugsnag using the Unity UI then perform the following:

Open the directory in the Project tab to `Assets/Standard Assets/Bugsnag` so that you can see the `BugsnagBehaviour` file.

Find the `GameObject` in the scene that Bugsnag has been added to, you should see in the inspector that the old Bugsnag script shows 'Missing (Mono Script)'.

Drag the `BugsnagBehaviour` file to the `Missing (Mono Script)` location in the inspector. This should apply your old Bugsnag API key to the script.

### Update code that uses Bugsnag

If you used the code-only configuration then perform the following:

```diff
- GameObject bugsnagObject = new GameObject("Bugsnag");
- GameObject.DontDestroyOnLoad(bugsnagObject);
- Bugsnag.createBugsnagInstance(bugsnagObject, "your-api-key-here");
+ BugsnagUnity.Bugsnag.Init("your-api-key-here");
```

If you used `MapUnityLogToSeverity` in a prior version this has now been replaced with the ability to add callbacks which can be used to modify the severity of an error report. See the [docs](http://localhost:4567/platforms/unity/#sending-diagnostic-data) for more information.
