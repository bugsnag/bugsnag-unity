Upgrading
=========

## v5.0.0

v5.0.0 contains breaking changes to Bugsnag's API that improve the reliability of the SDK and resolve several painpoints.

### New recommended way for initializing Bugsnag

The API has changed for initializing Bugsnag. It is now recommended that you supply all your configuration options up-front, and then call `Bugsnag.start()`:

```c#
Configuration config = new Configuration("your-api-key");
// alter all configuration options here, before Bugsnag.Start()
config.ReleaseStage = "beta"

// initialize Bugsnag
Bugsnag.Start(config);
```

If you initialize Bugsnag using a `GameObject` then no migration is necessary - the `BugsnagBehaviour.cs` script will be copied over when importing the `BugsnagUnity.package`.

### Configuration options must be supplied before Bugsnag.Start()

If you alter Bugsnag's default behaviour via `Configuration`, you must supply all values before calling `Bugsnag.Start()`.

Any change to the value of `Configuration` options after `Bugsnag.Start()` is called will have no effect on Bugsnag's behaviour.

## 4.1 to 4.2

4.2.0 adds support for reporting C/C++ crashes in Android code. If you are using
the Gradle build system to export your app, you will need to add the following
line to your exported project's build.gradle to enable this functionality:

### Gradle 4.2+

```groovy
dependencies {
  // ...
  // add this line:
	implementation(name: 'bugsnag-android-ndk-release', ext:'aar')
}
```

### Gradle 2.10

```groovy
dependencies {
  // ...
  // add this line:
	compile(name: 'bugsnag-android-ndk-release', ext:'aar')
}
```

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

If you used `MapUnityLogToSeverity` in a prior version this has now been replaced with the ability to add callbacks which can be used to modify the severity of an error report. See the [docs](https://docs.bugsnag.com/platforms/unity/#sending-diagnostic-data) for more information.
