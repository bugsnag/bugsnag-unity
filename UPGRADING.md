Upgrading
=========

## 7.x to 8.x

`Configuration.DiscardClasses` and `Configuration.RedactedKeys` are now [Regex](https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex?view=net-8.0) collections instead of string collections. This allows developers to have more control over how they perform. 

If you are using the `DiscardClasses` and `RedactedKeys` sections of the Bugsnag Unity Configuration Window, you can enter Regex patterns as strings and they will be converted into Regex objects when the Bugsnag SDK is started.

`Event.Unhandled` (accessed via OnError and OnSend callbacks) is now non null.

## 6.x to 7.x

When building using Unity 2019+, the Bugsnag SDK now uses a new method to intercept uncaught C# exceptions. This allows us access to the original exception object, meaning more accurate exception data and full support for inner exceptions.

This has slightly changed the format used for method names and signatures, which means that uncaught C# exceptions reported by `bugsnag-unity` V7+ will not group automatically in the Bugsnag dashboard with the equivalent exception reported by older versions of the library. 

The impact of this will be that some errors will effectively be duplicated between older and newer versions of your app.

Handled errors and native crashes will maintain their existing groups and are not affected by this upgrade.

#### Deprecation summary

The following methods have been removed from the `Bugsnag` client:

`Bugsnag.SessionTracking;` Please see the [capturing sessions](https://docs.bugsnag.com/platforms/unity/capturing-sessions/) section of our documentation for details on working with sessions. 

`Bugsnag.Send(IPayload payload);`


## 5.x to 6.x

### Key points

- Bugsnag can now be installed via [Unity Package Manager](https://docs.unity3d.com/Manual/Packages.html) (UPM) as well as from the package file published on GitHub.
- The file structure of the plugin has changed to keep all Bugsnag files together in a parent directory, so the files from the V5 package must be manually removed.
- Bugsnag can now start automatically before the first scene is loaded, removing the need for GameObject configuration, which has now been removed.
- The `BeforeNotify` callback has been replaced with an all-new callback system that offers access to all event types on all platforms for amending and discarding events before they are sent.
- The client (`Bugsnag`) API has been amended to improve usability and consistency.
- The `Context` is no longer set to the current scene name by default

More on each of these changes below:

### Unity Package Manager

You can now install Bugsnag using the [Unity Package Manager](https://docs.unity3d.com/Manual/Packages.html) (UPM) as well from the package file published on GitHub. See our [online docs](https://docs.bugsnag.com/platforms/unity#installation) for full instructions.

### Modified package file structure

All the files in the V6 package are located in a parent Bugsnag directory and so you will need to remove all the v5 files from your project to avoid having multiple instances of Bugsnag running.

To remove these files, you can download and run the bash script `utils/remove-bugsnag-v5.sh` in the `bugsnag-unity` repository, passing the path to your project as a parameter.

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

### Automatic initialization and removal of GameObject configuration

Bugsnag can now be started automatically just before the first scene is loaded. To successfully report events to your Bugsnag dashboard you will need to go to Window -> Bugsnag -> Settings and enter your API key and any other configuration options you require.

You can disable automatic start in the Settings window and initialize Bugsnag either fully or partially in code with `Bugsnag.Start(Configuration config)`. See our [online docs](https://docs.bugsnag.com/platforms/unity/#basic-configuration) for full details.

Starting Bugsnag via a GameObject has been removed. If you previously used this technique to configure Bugsnag then you will need to set the equivalent options in Window -> Bugsnag -> Settings. You will also need to remove your existing GameObject as the script it used is no longer available.

### New callback functionality

In v5.x, we offered a single callback type called `BeforeNotify` this was triggered for C# and JVM exceptions only. In v6.x, this callback has been renamed `OnError` and two further callback types have been added:

An `Bugsnag.AddOnSendError()` allows you to add a C# function that will be executed when the error report is about to be delivered to your Bugsnag dashboard. This is triggered for **all** event types – including native crashes – but this could be some time after the error/crash. For example, crash reports will only be delivered when the user next opens your app and has network connectivity. Therefore this callback type is useful for redacting/updating the event data that was captured but care should be taken adding data from the app or device as it will be in a later launch.

`Bugsnag.AddOnSession()` is also provided to allow modification and discarding of session data. It is triggered soon after the app starts.

See our [online docs](https://docs.bugsnag.com/platforms/unity/customizing-error-reports/) for further information.

### Default context
The `Context` of an event is no longer set by default to the last loaded scene name. This will change how errors are displayed on your dashboard and may affect the grouping of errors if you are using the "Group by error context" project setting. If you want to continue having `Context` set to the last loaded scene name you should set `Bugsnag.Context = <scene name>;` on scene loads.

The last loaded scene name is now captured in metadata as `app.lastLoadedUnityScene`. 

### Bugsnag client API changes

#### Metadata

The following methods have been added to the `Bugsnag` client to replace direct access to `Bugsnag.Metadata`:

| Property/Method                                                    | Usage                                                             |
| ------------------------------------------------------------------ | ----------------------------------------------------------------- |
| `AddMetadata(string section, string key, object value)`            | Add a specific value by key to a section of metadata              |
| `AddMetadata(string section, Dictionary<string, object> metadata)` | Add a section by name to the metadata metadata                    |
| `ClearMetadata(string section)`                                    | Clear a section of metadata                                       |
| `ClearMetadata(string section, string key)`                        | Clear a specific entry in a metadata section by key               |
| `Dictionary<string, object> GetMetadata(string section)`           | Get an entire section from the metadata as a Dictionary           |
| `object GetMetadata(string section, string key)`                   | Get an specific entry from the metadata as an object              |

See our [online docs](https://docs.bugsnag.com/platforms/unity/customizing-error-reports/#metadata) for full details.

#### Breadcrumbs

Creation of breadcrumbs outside of the Bugsnag library has now been removed and the parameter order for leaving a breadcrumb with metadata has been adjusted for consistency with our native SDKs.

Breadcrumbs can now be left using `Bugsnag.LeaveBreadcrumb(message)` or the more advanced `Bugsnag.LeaveBreadcrumb(message, metadata, type)` (see our online docs LINK_DOCS).

This 30-character limit on the message has now been removed.

See our [online docs](https://docs.bugsnag.com/platforms/unity/customizing-breadcrumbs/) for full details.

#### Deprecation summary

The following methods have been replaced in the `Bugsnag` client:

| v5.x API                                                           | v6.x API                                                          |
| ------------------------------------------------------------------ | ----------------------------------------------------------------- |
| `Bugsnag.BeforeNotify`                                             | Replaced by `Bugsnag.AddOnError(callback) or Bugsnag.AddOnSendError(callback)` - see our [online docs](https://docs.bugsnag.com/platforms/unity/customizing-error-reports/#updating-events-using-callbacks) |
| `Bugsnag.LeaveBreadcrumb(message, type, metadata)`                 | Replaced by `Bugsnag.LeaveBreadcrumb(message, metadata, type)`    |
| `Bugsnag.LeaveBreadcrumb(breadcrumb)`                              | Replaced by `Bugsnag.LeaveBreadcrumb(message, metadata, type)`    |
| `Bugsnag.Metadata`                                                 | Replaced by Add/Clear/Get operations – see above.                 |
| `Bugsnag.SetAutoNotify(bool)`                                      | Replaced by setting the config values [`AutoDetectErrors`](https://docs.bugsnag.com/platforms/unity/configuration-options/#auto-detect-errors) or [`EnabledErrorTypes`](https://docs.bugsnag.com/platforms/unity/configuration-options/#enabled-breadcrumb-types) in configuration or using an OnSendError callback|
| `Bugsnag.SetAutoDetectErrors(bool)`                                | Replaced by setting the config values [`AutoDetectErrors`](https://docs.bugsnag.com/platforms/unity/configuration-options/#auto-detect-errors) or [`EnabledErrorTypes`](https://docs.bugsnag.com/platforms/unity/configuration-options/#enabled-breadcrumb-types) in configuration or using an OnSendError callback|
| `Bugsnag.SetAutoDetectAnrs(bool)`                                  | Replaced by setting the config values [`AutoDetectErrors`](https://docs.bugsnag.com/platforms/unity/configuration-options/#auto-detect-errors) or [`EnabledErrorTypes`](https://docs.bugsnag.com/platforms/unity/configuration-options/#enabled-breadcrumb-types) in configuration or using an OnSendError callback|
| `Bugsnag.SetContext()`                                             | Replaced by `Bugsnag.Context {get; set;}`|
| `Bugsnag.StopSession()`                                            | Replaced by `Bugsnag.PauseSession()`|

The following methods have been replaced in the `Configuration` class:

| v5.x API                                                           | v6.x API                                                          |
| ------------------------------------------------------------------ | ----------------------------------------------------------------- |
| `AutoCaptureSessions`                                              | Replaced by [`AutoTrackSessions`](https://docs.bugsnag.com/platforms/unity/configuration-options/#auto-track-sessions) |
| `AutoDetectAnrs`                                                   | Replaced by [`EnabledErrorTypes`](https://docs.bugsnag.com/platforms/unity/configuration-options/#enabled-breadcrumb-types) |
| `AutoNotify`                                                       | Replaced by [`AutoDetectErrors`](https://docs.bugsnag.com/platforms/unity/configuration-options/#auto-detect-errors) |
| `Endpoint` and `SessionEndpoint`                                   | Replaced by [`Endpoints`](https://docs.bugsnag.com/platforms/unity/configuration-options/#endpoints) |
| `NotifyLevel`                                                      | Replaced by [`NotifyLogLevel`](https://docs.bugsnag.com/platforms/unity/configuration-options/#notify-log-level) |
| `NotifyReleaseStages`                                              | Replaced by [`EnabledReleaseStages`](https://docs.bugsnag.com/platforms/unity/configuration-options/#enabled-release-stages) |
| `ReportUncaughtExceptionsAsHandled`                                | Replaced by [`ReportExceptionLogsAsHandled`](https://docs.bugsnag.com/platforms/unity/configuration-options/#report-exception-logs-as-handled) |
| `UniqueLogsTimePeriod`                                             | Replaced by [`SecondsPerUniqueLog`](https://docs.bugsnag.com/platforms/unity/configuration-options/#seconds-per-unique-log) |

## 4.x to v5.x

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
