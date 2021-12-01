Upgrading
=========


## Upgrade from 5.X to 6.X

v6.0.0 contains breaking changes to Bugsnag's API that improve the reliability of the SDK and resolve several painpoints.

### Key points

- Bugsnag can now be installed via [Unity Package Manager](https://docs.unity3d.com/Manual/Packages.html) (UPM) as well from the package file published on Github.
- The file structure of the plugin has changed and so before installing please follow the instructions below on how to remove the old version.
- Starting Bugsnag via a GameObject has been removed in favor of Bugsnag starting automatically just before the first scene is loaded. Your Bugsnag settings can now be entered via the Bussnag settings window found at Window>Bugsnag>Settings.
- Metadata access has changed to make it clearer how to add, clear and get global and event metadata.
- BeforeNotify callbacks have been replaced with an all new callback system that offers much cleaner data access and data editing.  

More details of these changes can be found below and full documentation is available [online](https://docs.bugsnag.com/platforms/unity).


### Removing old Bugsnag files from your Unity project

In V6 of the Bugsnag SDK, all files are located in a parent Bugsnag directory. This means that, before installing V6 you must first remove all V5 files.

To assist in this you can download and run the bash script utils/remove-bugsnag-v5.sh, passing the path to your project as a parameter. This will remove all parts of Bugsnag V5.

If you wish to do it manually, please remove the following directories and files:

- File: `Assets/Plugins/Android/BugsnagUnity.Android.dll`
- File: `Assets/Plugins/Android/bugsnag-android-ndk-release.aar`
- File: `Assets/Plugins/Android/bugsnag-android-release.aar`
- File: `Assets/Plugins/Android/bugsnag-android-unity-release.aar`
- File: `Assets/Plugins/Android/bugsnag-android-anr-release.aar`
- File: `Assets/Plugins/Android/kotlin-annotations.jar`
- File: `Assets/Plugins/Android/kotlin-stdlib.jar`
- File: `Assets/Plugins/Android/kotlin-stdlib-common.jar`
- File: `Assets/Plugins/iOS/BugsnagUnity.iOS.dll`
- File: `Assets/Plugins/OSX/BugsnagUnity.MacOS.dll`
- File: `Assets/Plugins/tvOS/BugsnagUnity.iOS.dll`
- File: `Assets/Plugins/Windows/BugsnagUnity.Windows.dll`
- Directory: `Assets/Plugins/iOS/Bugsnag`
- Directory: `Assets/Plugins/OSX/Bugsnag`
- Directory: `Assets/Plugins/tvOS/Bugsnag`
- Directory: `Assets/Standard Assets/Bugsnag`


### New recommended way for initializing Bugsnag

#### Automatic initialization before the first scene is loaded

Starting Bugsnag via a GameObject has been removed in favor of Bugsnag starting automatically just before the first scene is loaded. 

To setup your Bugsnag configuration, please open the window by going to Window/Bugsnag/Settings. This will show the Bugsnag settings menu where you can add your API key and other settings.

It is still possible to initialise in code with `Bugsnag.Start(Configuration config)`.

### Metadata

#### Additions

The following methods have been added to the `Bugsnag` class: 

| Property/Method                                                    | Usage                                                             |
| ------------------------------------------------------------------ | ----------------------------------------------------------------- |
| `AddMetadata(string section, string key, object value)`            | Add a specific value by key to a section of metadata LINK_DOCS    | 
| `AddMetadata(string section, Dictionary<string, object> metadata)` | Add a section by name to the metadata metadata LINK_DOCS          | 
| `ClearMetadata(string section)`                                    | Clear a section of metadata LINK_DOCS          | 
| `ClearMetadataClearMetadata(string section, string key)`           | Clear a specific entry in a metadata section by key LINK_DOCS          | 
| `Dictionary<string, object> Bugsnag.GetMetadata(string section)`           | Get an entire section from the metadata as a Dictionary LINK_DOCS          | 
| `object Bugsnag.GetMetadata(string section, string key)`           | Get an specific entry from the metadata as an object LINK_DOCS          | 

#### Deprecations

The following property has been removed from the `Bugsnag` client:

| v5.x API                                                           | v6.x API                                                          |
| ------------------------------------------------------------------ | ----------------------------------------------------------------- |
| `Bugsnag.Metadata`                                                 | Deprecated - no longer public API.                                |


### Breadcrumbs

#### Deprecations

The following properties/methods have been removed from the `Bugsnag` client:

| v5.x API                                                           | v6.x API                                                          |
| ------------------------------------------------------------------ | ----------------------------------------------------------------- |
| `Bugsnag.LeaveBreadcrumb(message)`                                 | Replaced by `Bugsnag.LeaveBreadcrumb(message, metadata, type)`    |
| `Bugsnag.LeaveBreadcrumb(message, type, metadata)`                 | Replaced by `Bugsnag.LeaveBreadcrumb(message, metadata, type)`    |
| `Bugsnag.LeaveBreadcrumb(breadcrumb)`                              | Replaced by `Bugsnag.LeaveBreadcrumb(message, metadata, type)`    |


### Callbacks

#### Additions

The following methods have been added to the `Bugsnag` class: 

| Property/Method                                                    | Usage                                                             |
| ------------------------------------------------------------------ | ----------------------------------------------------------------- |
| `AddOnError` | Add a callback to modify or discard a event before it is recorded.  LINK_DOCS   | 
|  `AddOnSendError`  | Add a callback to modify or discard a event before it is sent (including native Cocoa and Android events).  LINK_DOCS   | 
| `AddOnSession` | Add a callbacks to modify or discard sessions before they are recorded and sent.  LINK_DOCS   | 

 
#### Deprecations

The following property has been removed from the `Bugsnag` client:

| v5.x API                                                           | v6.x API                                                          |
| ------------------------------------------------------------------ | ----------------------------------------------------------------- |
| `Bugsnag.BeforeNotify`                                             | Replaced by `Bugsnag.AddOnSendError(callback) or Bugsnag.AddOnError(callback)`|

### Misc

#### Deprecations

The following methods have been removed from the `Bugsnag` client:

| v5.x API                                                           | v6.x API                                                          |
| ------------------------------------------------------------------ | ----------------------------------------------------------------- |
| `Bugsnag.SetAutoNotify(bool)`                                      | Deprecated - no longer public API.  Callbacks should now be used to stop sending particular errors at runtime|
| `Bugsnag.SetAutoDetectErrors(bool)`                                      | Deprecated - no longer public API.  Callbacks should now be used to stop sending particular errors at runtime|
| `Bugsnag.SetAutoDetectAnrs(bool)`                                  | Deprecated - no longer public API.  Callbacks should now be used to stop sending particular errors at runtime|
| `Bugsnag.StopSession()`                                              | Replaced by `Bugsnag.PauseSession()`|
| `Bugsnag.SetContext()`                                              | Replaced by `Bugsnag.Context {get; set;}`|


## v5.0.0

v5.0.0 contains breaking changes to Bugsnag's API that improve the reliability of the SDK and resolve several painpoints.

### New recommended way for initializing Bugsnag

#### GameObject initialization

If you initialize Bugsnag using a `GameObject` and only configure Bugsnag via the Unity Inspector UI then no migration is necessary. The necessary changes will be copied over in the `BugsnagBehaviour` script when you import the `Bugsnag.unitypackage`.

#### Code initialization

If you initialize Bugsnag in code then a migration is required. You should replace any call to `Bugsnag.Init()` with `Bugsnag.Start()`.

It is also now necessary to supply all your configuration options up-front, and pass them in as a parameter to `Bugsnag.Start()`:

```c#
Configuration config = new Configuration("your-api-key");
// alter all configuration options here, before Bugsnag.Start()
config.ReleaseStage = "beta"

// initialize Bugsnag
Bugsnag.Start(config);
```

### Configuration options must be supplied before Bugsnag.Start()

If you alter Bugsnag's default behaviour via `Configuration`, you must supply all values before calling `Bugsnag.Start()`.

Any change to the value of `Configuration` options after `Bugsnag.Start()` is called will have no effect on Bugsnag's behaviour.

### Configuration constructor removed

The previous constructor for `Configuration` allowed passing `AutoNotify` as its 2nd parameter:

```c#
new Configuration("your-api-key", true);
```

This has been removed - `AutoNotify` should be set using the property instead:

```c#
Configuration config = new Configuration("your-api-key");
config.AutoNotify = true;
```

### Bugsnag.Configuration accessor removed

The `Bugsnag.Configuration` and `Bugsnag.Client.Configuration` accessors have been removed. You should supply all your configuration options up-front as recommended [here](#new-recommended-way-for-initializing-bugsnag).

### AutoNotify, AutoDetectAnrs, and Context replaced

Previously it was possible to set `AutoNotify`, `AutoDetectAnrs` and `Context` after Bugsnag has initialized:

```c#
Bugsnag.Configuration.AutoNotify = false;
Bugsnag.Configuration.AutoDetectAnrs = false;
Bugsnag.Configuration.Context = "MyContext";
```

This has been replaced by the following API:

```c#
Bugsnag.SetAutoNotify(false);
Bugsnag.SetAutoDetectAnrs(false);
Bugsnag.SetContext("MyContext");
```

### Event payload changes

- (Android) Removed `packageName` from the app metadata tab, as the field is duplicated by `app.id`
- (Android) Removed `versionName` from the app metadata tab, as the field is duplicated by `app.version` and this has been known to cause confusion amongst users in the past

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
