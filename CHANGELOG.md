# Changelog

## 6.3.1 (2022-04-06)

### Enhancements

* Update bugsnag-android to v5.22.0
  * Added `Bugsnag.isStarted()` to test whether the Bugsnag client is in the middle of initializing. This can be used to guard uses of the Bugsnag API that are either on separate threads early in the app's start-up and so not guaranteed to be executed after `Bugsnag.start` has completed, or where Bugsnag may not have been started at all due to some internal app logic.
    [slack-jallen](https://github.com/slack-jallen):[#1621](https://github.com/bugsnag/bugsnag-android/pull/1621)
    [bugsnag-android#1640](https://github.com/bugsnag/bugsnag-android/pull/1640)
  * Events and Sessions will be discarded if they cannot be uploaded and are older than 60 days or larger than 1MB
    [bugsnag-android#1633](https://github.com/bugsnag/bugsnag-android/pull/1633)
  * Fixed potentially [thread-unsafe access](https://github.com/bugsnag/bugsnag-android/issues/883) when invoking `Bugsnag` static methods across different threads whilst `Bugsnag.start` is still in-flight. It is now safe to call any `Bugsnag` static method once `Bugsnag.start` has _begun_ executing, as access to the client singleton is controlled by a lock, so the new `isStarted` method (see above) should only be required where it cannot be determined whether the call to `Bugsnag.start` has begun or you do not want to wait.
    [bugsnag-android#1638](https://github.com/bugsnag/bugsnag-android/pull/1638)
  * Calling `bugsnag_event_set_context` with NULL `context` correctly clears the event context again
    [bugsnag-android#1637](https://github.com/bugsnag/bugsnag-android/pull/1637)

### Bug fixes

* Fixed an issue where the use of ToUpper caused a crash on devices using the Turkish language
  [#543](https://github.com/bugsnag/bugsnag-unity/pull/543)

* Fixed an issue where breadcrumbs with null messages caused errors
  [#545](https://github.com/bugsnag/bugsnag-unity/pull/545)

## 6.3.0 (2022-03-23)

### Enhancements

* Added Android support for [EDM4U](https://github.com/googlesamples/unity-jar-resolver). For manual installs there see the new menu item at Window/Bugsnag/Enable EDM Support. For UPM installs we have a [dedicated package](https://github.com/bugsnag/bugsnag-unity-upm-edm4u). [#528](https://github.com/bugsnag/bugsnag-unity/pull/528)

* Update bugsnag-android to v5.21.0
  * Fix inconsistencies in stack trace quality for C/C++ events. Resolves a few
    cases where file and line number information was not resolving to the correct
    locations. This change may result in grouping changes to more correctly
    highlight the root cause of an event.
    [#1605](https://github.com/bugsnag/bugsnag-android/pull/1605)
    [#1606](https://github.com/bugsnag/bugsnag-android/pull/1606)
  * Fixed an issue where an uncaught exception on the main thread could in rare cases trigger an ANR.
    [#1624](https://github.com/bugsnag/bugsnag-android/pull/1624)

## 6.2.0 (2022-03-16)

### Enhancements

* Added offline persistence of C# events/exceptions (all platforms); and of sessions and device ID (Windows and WebGL) [#512](https://github.com/bugsnag/bugsnag-unity/pull/512) [#509](https://github.com/bugsnag/bugsnag-unity/pull/509) [#514](https://github.com/bugsnag/bugsnag-unity/pull/514)

* Add `Configuration.MaxReportedThreads` config option to set the [native Android option](https://docs.bugsnag.com/platforms/android/configuration-options/#maxreportedthreads) [523](https://github.com/bugsnag/bugsnag-unity/pull/523)

* Update bugsnag-android to v5.20.0
  * The number of threads reported can now be limited using `Configuration.setMaxReportedThreads` (defaulting to 200)
    [bugsnag-android#1607](https://github.com/bugsnag/bugsnag-android/pull/1607)
  * Improved the performance and stability of the NDK and ANR plugins by caching JNI references on start
    [bugsnag-android#1596](https://github.com/bugsnag/bugsnag-android/pull/1596)
    [bugsnag-android#1601](https://github.com/bugsnag/bugsnag-android/pull/1601)

## 6.1.0 (2022-02-08)

### Enhancements

* New APIs to support forthcoming feature flag and experiment functionality.
  For more information, please see https://docs.bugsnag.com/product/features-experiments
  [#504](https://github.com/bugsnag/bugsnag-unity/pull/504) [#501](https://github.com/bugsnag/bugsnag-unity/pull/501)

* Update bugsnag-cocoa to v6.16.1
  * New APIs to support forthcoming feature flag and experiment functionality.
    For more information, please see https://docs.bugsnag.com/product/features-experiments
    [bugsnag-cocoa#1279](https://github.com/bugsnag/bugsnag-cocoa/pull/1279)
  * Fix missing user.id in OOM events with no active session.
    [bugsnag-cocoa#1274](https://github.com/bugsnag/bugsnag-cocoa/pull/1274)
  * Improve crash report writing performance and size on disk.
    [bugsnag-cocoa#1273](https://github.com/bugsnag/bugsnag-cocoa/pull/1273)
    [bugsnag-cocoa#1281](https://github.com/bugsnag/bugsnag-cocoa/pull/1281)
  * Detect hangs during launch of UIScene based apps.
    [bugsnag-cocoa#1263](https://github.com/bugsnag/bugsnag-cocoa/pull/1263)
  * Stop persisting changes made by `OnSendError` callbacks if delivery needs to be retried.
    [bugsnag-cocoa#1262](https://github.com/bugsnag/bugsnag-cocoa/pull/1262)
  * Fix incorrect `device.freeDisk` in crash errors.
    [bugsnag-cocoa#1256](https://github.com/bugsnag/bugsnag-cocoa/pull/1256)
  * Fix some potential deadlocks that could occur if a crash handler crashes.
    [bugsnag-cocoa#1252](https://github.com/bugsnag/bugsnag-cocoa/pull/1252)
  * Fix `UIApplicationState` detection when started from a SwiftUI app's `init()` function.
    This fixes false positive OOMs on iOS 15 for apps that have been prewarmed without transitioning to the foreground.
    [bugsnag-cocoa#1248](https://github.com/bugsnag/bugsnag-cocoa/pull/1248)
  * Load configuration from the plist instead of using defaults when calling Bugsnag.start(withApiKey:)
    [bugsnag-cocoa#1245](https://github.com/bugsnag/bugsnag-cocoa/pull/1245)
  * New APIs to allow `OnBreadcrumb`, `OnSendError` and `OnSession` Swift closures to be removed.
    The following APIs are now deprecated and will be removed in the next major release:
    * `removeOnBreadcrumb(block:)`
    * `removeOnSendError(block:)`
    * `removeOnSession(block:)`
    [bugsnag-cocoa#1240](https://github.com/bugsnag/bugsnag-cocoa/pull/1240)
  * Include metadata in breadcrumbs for `UIWindow` / `NSWindow` notifications.
    [bugsnag-cocoa#1238](https://github.com/bugsnag/bugsnag-cocoa/pull/1238)
  * Fix a crash in `-[BSGURLSessionTracingDelegate URLSession:task:didFinishCollectingMetrics:]` for tasks with no request.
    [bugsnag-cocoa#1230](https://github.com/bugsnag/bugsnag-cocoa/pull/1230)
  * Use `LC_FUNCTION_STARTS` to improve symbolication accuracy.
    [bugsnag-cocoa#1214](https://github.com/bugsnag/bugsnag-cocoa/pull/1214)
  * Fix missing imports when building with `CLANG_ENABLE_MODULES=NO`
    [bugsnag-cocoa#1284](https://github.com/bugsnag/bugsnag-cocoa/pull/1284)

* Update bugsnag-android to v5.19.2
  * New APIs to support forthcoming feature flag and experiment functionality. For more information, please see https://docs.bugsnag.com/product/features-experiments.
  * Explicitly define Kotlin api/language versions
    [bugsnag-android#1564](https://github.com/bugsnag/bugsnag-android/pull/1564)
  * Build project with Kotlin 1.4, maintain compat with Kotlin 1.3
    [bugsnag-android#1565](https://github.com/bugsnag/bugsnag-android/pull/1565)
  * Discarded unhandled exceptions are propagated to any previously registered handlers
    [bugsnag-android#1584](https://github.com/bugsnag/bugsnag-android/pull/1584)
  * Fix SIGABRT crashes caused by race conditions in the NDK layer
    [bugsnag-android#1585](https://github.com/bugsnag/bugsnag-android/pull/1585)
  * Fixed an issue where feature-flags were not always sent if an OnSendCallback was configured
    [bugsnag-android#1589](https://github.com/bugsnag/bugsnag-android/pull/1589)
  * Fix a bug where api keys set in React Native callbacks were ignored
    [bugsnag-android#1592](https://github.com/bugsnag/bugsnag-android/pull/1592)


## 6.0.0 (2022-01-20)

This version contains **breaking** changes, as bugsnag-unity has been updated to allow for more convenient and performant initialisation and configuration features.

Please see the [upgrade guide](./UPGRADING.md) for details of all the changes and instructions on how to upgrade.

In addition to the changes mentioned in the upgrade guide, the bundled Bugsnag Android Notifier has been updated. See below for details.

* Update bugsnag-android to v5.18.0
  * Bump compileSdkVersion to apiLevel 31
    [bugsnag-android#1536](https://github.com/bugsnag/bugsnag-android/pull/1536)
  * Flush in-memory sessions first
    [bugsnag-android#1538](https://github.com/bugsnag/bugsnag-android/pull/1538)
  * Avoid unnecessary network connectivity change breadcrumb
    [bugsnag-android#1540](https://github.com/bugsnag/bugsnag-android/pull/1540)
    [bugsnag-android#1546](https://github.com/bugsnag/bugsnag-android/pull/1546)
  * Clear native stacktrace memory in `bugsnag_notify_env` before attempting to unwind the stack
    [bugsnag-android#1543](https://github.com/bugsnag/bugsnag-android/pull/1543)
  * Increase resilience of NDK stackframe method capture
    [bugsnag-android#1484](https://github.com/bugsnag/bugsnag-android/pull/1484)
  * `redactedKeys` now correctly apply to metadata on Event breadcrumbs
    [bugsnag-android#1526](https://github.com/bugsnag/bugsnag-android/pull/1526)
  * Improved the robustness of automatically logged `ERROR` breadcrumbs
    [bugsnag-android#1531](https://github.com/bugsnag/bugsnag-android/pull/1531)
  * Improve performance on the breadcrumb storage "hot path" by removing Date formatting
    [bugsnag-android#1525](https://github.com/bugsnag/bugsnag-android/pull/1525)
  * Improve the memory use and performance overhead when handling the delivery response status codes
    [bugsnag-android#1558](https://github.com/bugsnag/bugsnag-android/pull/1558)
  * Harden ndk layer through use of const keyword
    [bugsnag-android#1566](https://github.com/bugsnag/bugsnag-android/pull/1566)
  * Delete persisted NDK events earlier in delivery process
    [bugsnag-android#1562](https://github.com/bugsnag/bugsnag-android/pull/1562)
  * Add null checks for strlen()
    [bugsnag-android#1563](https://github.com/bugsnag/bugsnag-android/pull/1563)
  * Catch IOException when logging response status code
    [bugsnag-android#1567](https://github.com/bugsnag/bugsnag-android/pull/1567)

## 5.4.2 (2021-11-16)

* Update bugsnag-cocoa to v6.14.2
  * Fix missing `configuration.user` and manually resumed `session` info in unhandled errors.
    [bugsnag-cocoa#1215](https://github.com/bugsnag/bugsnag-cocoa/pull/1215)
* Update bugsnag-android to v5.15.0 
  * Avoid reporting false-positive background ANRs with improved foreground detection
    [bugsnag-android#1429](https://github.com/bugsnag/bugsnag-android/pull/1429)
  * Prevent events being attached to phantom sessions when they are blocked by an `OnSessionCallback`
    [bugsnag-android#1434](https://github.com/bugsnag/bugsnag-android/pull/1434)
  * Plugins will correctly mirror metadata added using `addMetadata(String, Map)`
    [bugsnag-android#1454](https://github.com/bugsnag/bugsnag-android/pull/1454)

### Bug fixes

* Fixed an issue where breadcrumbs from non fatal apphang errors caused a crash on retrieval
  [#431](https://github.com/bugsnag/bugsnag-unity/pull/431)

## 5.4.1 (2021-10-25)

### Enhancements

* Removed the limit on the length of a breadcrumbs name [#399](https://github.com/bugsnag/bugsnag-unity/pull/399)

* Update bugsnag-android to v5.14.0
  * Capture and report thread state (running, sleeping, etc.) for Android Runtime and Native threads
    [bugsnag-android#1367](https://github.com/bugsnag/bugsnag-android/pull/1367)
    [bugsnag-android#1390](https://github.com/bugsnag/bugsnag-android/pull/1390)

* Update bugsnag-cocoa to v6.14.1
  * Disable automatic session tracking in app extensions (it was not working as intended.)
    [bugsnag-cocoa#1211](https://github.com/bugsnag/bugsnag-cocoa/pull/1211)
  * Stop logging "[ERROR] Failed to install crash handler..." if a debugger is attached.
    [bugsnag-cocoa#1210](https://github.com/bugsnag/bugsnag-cocoa/pull/1210)
  * Include the word "request" in network request breadcrumb messages.
    [bugsnag-cocoa#1209](https://github.com/bugsnag/bugsnag-cocoa/pull/1209)
  * Apply `redactedKeys` to breadcrumb metadata.
    [bugsnag-cocoa#1204](https://github.com/bugsnag/bugsnag-cocoa/pull/1204)
  * Capture and report thread states (running, stopped, waiting, etc.) 
    [bugsnag-cocoa#1200](https://github.com/bugsnag/bugsnag-cocoa/pull/1200)
  * Disable app hang detection for app extensions.
    [bugsnag-cocoa#1198](https://github.com/bugsnag/bugsnag-cocoa/pull/1198)

### Bug fixes

* Fixed an issue where csharp exceptions originating from background threads caused errors in the App and Device class constructors
  [#413](https://github.com/bugsnag/bugsnag-unity/pull/413)

## 5.4.0 (2021-09-27)

### Enhancements

* Update bugsnag-cocoa to v6.12.2

  * Stop dropping breadcrumbs when provided invalid metadata (that is not JSON convertible.)
    [bugsnag-cocoa#1187](https://github.com/bugsnag/bugsnag-cocoa/pull/1187)

  * Fix Swift fatal error parsing for messages with no filename.
    [bugsnag-cocoa#1186](https://github.com/bugsnag/bugsnag-cocoa/pull/1186)

  * Events now include a `thermalState` property in the `device` tab, and unexpected app terminations that occur
    when the thermal state is critical will now be reported as a "Thermal Kill" rather than Out Of Memory error.
    [bugsnag-cocoa#1171](https://github.com/bugsnag/bugsnag-cocoa/pull/1171)

  * Fix a regression where the session was not captured at launch if Bugsnag was started before
    `willFinishLaunchingWithOptions` in iOS apps that do not adopt the UIScene life-cycle.
    [bugsnag-cocoa#1180](https://github.com/bugsnag/bugsnag-cocoa/pull/1180)

  * Fatal app hangs will no longer be reported if the `willTerminateNotification` is received.
    [bugsnag-cocoa#1176](https://github.com/bugsnag/bugsnag-cocoa/pull/1176)
    
* Update bugsnag-android to v5.13.0
  * The `app.lowMemory` value always report the most recent `onTrimMemory`/`onLowMemory` status [bugsnag-android#1342](https://github.com/bugsnag/bugsnag-android/pull/1342)
  * Added the `app.memoryTrimLevel` metadata to report a description of the latest `onTrimMemory` status [bugsnag-android#1344](https://github.com/bugsnag/bugsnag-android/pull/1344)
  * Added `STATE` Breadcrumbs for `onTrimMemory` events [bugsnag-android#1345](https://github.com/bugsnag/bugsnag-android/)
  * Capture breadcrumbs for OkHttp network requests
    [bugsnag-android#1358](https://github.com/bugsnag/bugsnag-android/pull/1358)
    [bugsnag-android#1361](https://github.com/bugsnag/bugsnag-android/pull/1361)
    [bugsnag-android#1363](https://github.com/bugsnag/bugsnag-android/pull/1363)
    [bugsnag-android#1379](https://github.com/bugsnag/bugsnag-android/pull/1379)
  * Update project to build using Gradle/AGP 7
    [bugsnag-android#1354](https://github.com/bugsnag/bugsnag-android/pull/1354)
  * Increased default breadcrumb collection limit to 50
    [bugsnag-android#1366](https://github.com/bugsnag/bugsnag-android/pull/1366)
  * Support integer values in buildUuid
    [bugsnag-android#1375](https://github.com/bugsnag/bugsnag-android/pull/1375)
  * Use SystemClock.elapsedRealtime to track `app.durationInForeground`
    [bugsnag-android#1375](https://github.com/bugsnag/bugsnag-android/pull/1375)


* Add new automatically collected Device data to Windows, WebGL and Unity Editor events: `batteryLevel, charging, id, model, screenDensity, screenResolution, totalMemory` [#390](https://github.com/bugsnag/bugsnag-unity/pull/390)

* Add new automatically collected App data to Windows, WebGL and Unity Editor events: `duration, id, isLaunching, lowMemory` [#390](https://github.com/bugsnag/bugsnag-unity/pull/390)

* Add new Bugsnag.Notify overloads: `Notify(exception, stacktrace)`  `Notify(exception, stacktrace, callback)` `Notify(name, message, stacktrace)` `Notify(name, message, stacktrace, callback)` [#380](https://github.com/bugsnag/bugsnag-unity/pull/380)

* Add `Bugsnag.GetLastRunInfo()` To get relevant crash information regarding the last run of the application [#379](https://github.com/bugsnag/bugsnag-unity/pull/379)

* Add `Configuration.SendLaunchCrashesSynchronously` config option to set the [native Android option](https://docs.bugsnag.com/platforms/android/configuration-options/#launchdurationmillis) and the [native Cocoa option](https://docs.bugsnag.com/platforms/ios/configuration-options/#launchdurationmillis) [#379](https://github.com/bugsnag/bugsnag-unity/pull/379)

* Add `Configuration.LaunchDurationMillis` config option to set the [native Android option](https://docs.bugsnag.com/platforms/android/configuration-options/#sendlaunchcrashessynchronously) and the [native Cocoa option](https://docs.bugsnag.com/platforms/ios/configuration-options/#sendlaunchcrashessynchronously) [#379](https://github.com/bugsnag/bugsnag-unity/pull/379)

### Deprecated

* `Bugsnag.StopSession` has been deprecated in favour of `Bugsnag.PauseSession` and will be removed in the next major release.

### Bug fixes

* Fixed an issue where app.type was not sent in native Cocoa crash reports
  [#395](https://github.com/bugsnag/bugsnag-unity/pull/395)


## 5.3.0 (2021-09-01)

### Enhancements

* Add `Configuration.VersionCode` config option to set the [native Android option](https://docs.bugsnag.com/platforms/android/configuration-options/#versioncode) [#373](https://github.com/bugsnag/bugsnag-unity/pull/373)

* Add `Configuration.PersistenceDirectory` to set the [native Android option](https://docs.bugsnag.com/platforms/android/configuration-options/#persistencedirectory) [#368](https://github.com/bugsnag/bugsnag-unity/pull/368)

* Add `Configuration.SendThreads` config option to set the [native Android option](https://docs.bugsnag.com/platforms/android/configuration-options/#sendthreads) and the [native Cocoa option](https://docs.bugsnag.com/platforms/ios/configuration-options/#sendthreads) [#375](https://github.com/bugsnag/bugsnag-unity/pull/375)

* Add `Configuration.PersistUser` config option to set the [native Android option](https://docs.bugsnag.com/platforms/android/configuration-options/#persistuser) and the [native Cocoa option](https://docs.bugsnag.com/platforms/ios/configuration-options/#persistuser) [#372](https://github.com/bugsnag/bugsnag-unity/pull/372)

* Add `Configuration.MaxPersistedEvents` config option to set the [native Android option](https://docs.bugsnag.com/platforms/android/configuration-options/#maxpersistedevents) and the [native Cocoa option](https://docs.bugsnag.com/platforms/ios/configuration-options/#maxpersistedevents) [#371](https://github.com/bugsnag/bugsnag-unity/pull/371)

* Add `Configuration.RedactedKeys` configuration option to enable redacting specific keys in metadata [#367](https://github.com/bugsnag/bugsnag-unity/pull/367)

* Add `Configuration.Endpoints` to enable setting custom endpoints for events and sessions [#366](https://github.com/bugsnag/bugsnag-unity/pull/366)

* Add `Configuration.ProjectPackages` config option to set the [native Android option](https://docs.bugsnag.com/platforms/android/configuration-options/#projectpackages) [#364](https://github.com/bugsnag/bugsnag-unity/pull/364)

* Add `Configuration.BundleVersion` config option to set the [native Cocoa option](https://docs.bugsnag.com/platforms/ios/configuration-options/#bundleversion) [#359](https://github.com/bugsnag/bugsnag-unity/pull/359)

* Add `Configuration.AppType` configuration option to enable setting a custom value for the app.type field in an event [#363](https://github.com/bugsnag/bugsnag-unity/pull/363)

* Update bugsnag-cocoa to v6.11.0

  * Add breadcrumbs for `UIScene` notifications.
    [#1165](https://github.com/bugsnag/bugsnag-cocoa/pull/1165)

  * Fix another rare crash in `bsg_ksmachgetThreadQueueName`.
    [#1157](https://github.com/bugsnag/bugsnag-cocoa/pull/1157)

  * Fix ThreadSanitizer data race in `BugsnagBreadcrumbs`.
    [#1160](https://github.com/bugsnag/bugsnag-cocoa/pull/1160)
    
* Add `DiscardClasses` configuration option to disable sending events that contain user defined error classes. [#361](https://github.com/bugsnag/bugsnag-unity/pull/361)

* Update bugsnag-android to v5.11.0:
  * Add Bugsnag listeners for StrictMode violation detection
    [#1331](https://github.com/bugsnag/bugsnag-android/pull/1331)

  * Address pre-existing StrictMode violations
    [#1328](https://github.com/bugsnag/bugsnag-android/pull/1328)
    
### Deprecated

* `Bugsnag.SetContext(string context)` has been deprecated in favour of the new `Bugsnag.Context` property and will be removed in the next major release.

* `Configuration.Endpoint` has been deprecated in favour of the new `Configuration.Endpoints` class and will be removed in the next major release.

* `Configuration.SessionEndpoint` has been deprecated in favour of the new `Configuration.Endpoints` class and will be removed in the next major release.   

* `Configuration.NotifyReleaseStages` has been deprecated in favour of `Configuration.EnabledReleaseStages` and will be removed in the next major release. 

## 5.2.0 (2021-08-04)

### Enhancements

* Add `AppHangThresholdMillis` to set the [native Cocoa option](https://docs.bugsnag.com/platforms/ios/configuration-options/#apphangthresholdmillis) [#347](https://github.com/bugsnag/bugsnag-unity/pull/347)

* Add `EnabledErrorTypes` configuration option to enable/disable different types of errors [#341](https://github.com/bugsnag/bugsnag-unity/pull/341)

* [Android] Automatic [App Not Responding](https://developer.android.com/topic/performance/vitals/anr) (ANR) detection is now enabled by default  
  [#339](https://github.com/bugsnag/bugsnag-unity/pull/339)

* Update bugsnag-cocoa to v6.10.2

  * Fix ThreadSanitizer data race warning in `BSGAppHangDetector`.
    [#1153](https://github.com/bugsnag/bugsnag-cocoa/pull/1153)

  * Remove (duplicated) `user` information from `metaData`.
    [#1151](https://github.com/bugsnag/bugsnag-cocoa/pull/1151)

  * Fix a potential stack overflow in `+[BugsnagThread allThreadsWithCurrentThreadBacktrace:]`.
    [#1148](https://github.com/bugsnag/bugsnag-cocoa/pull/1148)

  * Fix `NSNull` handling in `+[BugsnagError errorFromJson:]` and `+[BugsnagStackframe frameFromJson:]`.
    [#1143](https://github.com/bugsnag/bugsnag-cocoa/pull/1143)

  * Fix a rare crash in `bsg_ksmachgetThreadQueueName`.
    [#1147](https://github.com/bugsnag/bugsnag-cocoa/pull/1147)

  * Fix an issue that could cause C++ exceptions with very long descriptions to not be reported.
    [#1137](https://github.com/bugsnag/bugsnag-cocoa/pull/1137)

  * Improve performance of adding metadata by using async file I/O.
    [#1133](https://github.com/bugsnag/bugsnag-cocoa/pull/1133)

  * Improve performance of leaving breadcrumbs by using async file I/O.
    [#1124](https://github.com/bugsnag/bugsnag-cocoa/pull/1124)

  * Prevent some potential false positive detection of app hangs.
    [#1122](https://github.com/bugsnag/bugsnag-cocoa/pull/1122)
    
* Update bugsnag-android to v5.10.1:

  * Prefer `calloc()` to `malloc()` in NDK code
    [#1320](https://github.com/bugsnag/bugsnag-android/pull/1320)

  * Ensure correct value always collected for activeScreen
    [#1322](https://github.com/bugsnag/bugsnag-android/pull/1322)

  * Capture process name in Event payload
    [#1318](https://github.com/bugsnag/bugsnag-android/pull/1318)

  * Avoid unnecessary BroadcastReceiver registration for monitoring device orientation
    [#1303](https://github.com/bugsnag/bugsnag-android/pull/1303)

  * Register system callbacks on background thread
    [#1292](https://github.com/bugsnag/bugsnag-android/pull/1292)

  * Fix rare NullPointerExceptions from ConnectivityManager
    [#1311](https://github.com/bugsnag/bugsnag-android/pull/1311)

  * Respect manual setting of context
    [#1310](https://github.com/bugsnag/bugsnag-android/pull/1310)

  * Handle interrupt when shutting down executors
    [#1315](https://github.com/bugsnag/bugsnag-android/pull/1315)

  * Allow serializing enabledBreadcrumbTypes as null
    [#1316](https://github.com/bugsnag/bugsnag-android/pull/1316)

  * Properly handle ANRs after multiple calls to autoNotify and autoDetectAnrs
    [#1265](https://github.com/bugsnag/bugsnag-android/pull/1265)

  * Cache value of app.backgroundWorkRestricted
    [#1275](https://github.com/bugsnag/bugsnag-android/pull/1275)

  * Optimize execution of callbacks
    [#1276](https://github.com/bugsnag/bugsnag-android/pull/1276)

  * Optimize implementation of internal state change observers
    [#1274](https://github.com/bugsnag/bugsnag-android/pull/1274)

  * Optimize metadata implementation by reducing type casts
    [#1277](https://github.com/bugsnag/bugsnag-android/pull/1277)

  * Trim stacktraces to <200 frames before attempting to construct POJOs
    [#1281](https://github.com/bugsnag/bugsnag-android/pull/1281)

  * Use direct field access when adding breadcrumbs and state updates
    [#1279](https://github.com/bugsnag/bugsnag-android/pull/1279)

  * Avoid using regex to validate api key
    [#1282](https://github.com/bugsnag/bugsnag-android/pull/1282)

  * Discard unwanted automatic data earlier where possible
    [#1280](https://github.com/bugsnag/bugsnag-android/pull/1280)

  * Enable ANR handling on immediately if started from the main thread
    [#1283](https://github.com/bugsnag/bugsnag-android/pull/1283)

  * Include `app.binaryArch` in all events
    [#1287](https://github.com/bugsnag/bugsnag-android/pull/1287)

  * Cache results from PackageManager
    [#1288](https://github.com/bugsnag/bugsnag-android/pull/1288)

  * Use ring buffer to store breadcrumbs
    [#1286](https://github.com/bugsnag/bugsnag-android/pull/1286)

  * Avoid expensive set construction in Config constructor
    [#1289](https://github.com/bugsnag/bugsnag-android/pull/1289)

  * Replace calls to String.format() with concatenation
    [#1293](https://github.com/bugsnag/bugsnag-android/pull/1293)

  * Optimize capture of thread traces
    [#1300](https://github.com/bugsnag/bugsnag-android/pull/1300)    
    
### Bug fixes

* Fixed an issue where Windows events would have incorrectly split stacktraces resulting in all lines being bundled into one 
  [#350](https://github.com/bugsnag/bugsnag-unity/pull/350)

* Fixed an issue where WebGL web requests that initially fail were not respecting the 10 second delay before retrying 
  [#321](https://github.com/bugsnag/bugsnag-unity/pull/321)
  
* Fixed an issue where Breadcrumbs were reported in the wrong order on Windows and in the Unity Editor
  [#322](https://github.com/bugsnag/bugsnag-unity/pull/322)

* Fixed an issue where a "Bugsnag loaded" breadcrumb was not added on Windows, Linux, WebGL, and editor builds
  [#327](https://github.com/bugsnag/bugsnag-unity/pull/327)

* Fixed an issue where the fallback was not reporting the correct app.type
  [#325](https://github.com/bugsnag/bugsnag-unity/pull/325)
  
### Deprecated

* `Configuration.NotifyLevel` has been deprecated in favour of `Configuration.NotifyLogLevel` and will be removed in the next major release.

* `Configuration.AutoNotify` has been deprecated in favour of `Configuration.AutoDetectErrors` and will be removed in the next major release.

* `Configuration.AutoCaptureSessions` has been deprecated in favour of `Configuration.AutoTrackSessions` and will be removed in the next major release.
 

## 5.1.1 (2021-06-24)

* Fixes a packaging issue in the 5.1.0 where the library version was incorrectly
  reported
  [#309](https://github.com/bugsnag/bugsnag-unity/pull/309)

## 5.1.0 (2021-06-24)

### Enhancements

* Add event metadata for CPU and graphics capabilities and migrated entries from the Unity tab to the device and app tabs, to better match other platforms [#297](https://github.com/bugsnag/bugsnag-unity/pull/297):
  * `unity.companyName` -> `app.companyName`
  * `unity.productName` -> `app.name`
  * `unity.version` -> `app.version`
  * `unity.platform` -> `app.type`
  * `unity.osLanguage` -> `device.osLanguage`
  * `unity.unityException` -> removed as a duplicate of the error class
  * `unity.unityLogType` -> removed as is contained in the error class for generic logs
	
* Add `EnabledBreadcrumbTypes` configuration option to enable/disable automatically recorded breadcrumbs [#301](https://github.com/bugsnag/bugsnag-unity/pull/301)
* Add `MaxBreadcrumbs` configuration option to control the number of breadcrumbs collected on all platforms
  [#275](https://github.com/bugsnag/bugsnag-unity/pull/275)
  [#304](https://github.com/bugsnag/bugsnag-unity/pull/304)

* Update bugsnag-cocoa to v6.9.6:
	
  * Improve accuracy of app hang event information to better reflect state at time of detection.
  [#1118](https://github.com/bugsnag/bugsnag-cocoa/pull/1118)
  
  * Stop app hangs being reported if app is launched in the background.
  [#1112](https://github.com/bugsnag/bugsnag-cocoa/pull/1112)

  * Stop session being reported if app is launched in the background.
  [#1107](https://github.com/bugsnag/bugsnag-cocoa/pull/1107)

  * Fix KSCrash state storage for apps with no CFBundleName.
  [#1103](https://github.com/bugsnag/bugsnag-cocoa/pull/1103)
  
  * Improve performance of `notify()`.
  [#1102](https://github.com/bugsnag/bugsnag-cocoa/pull/1102)
  [#1104](https://github.com/bugsnag/bugsnag-cocoa/pull/1104)
  [#1105](https://github.com/bugsnag/bugsnag-cocoa/pull/1105)

  * Fix a crash in `-[BugsnagApp deserializeFromJson:]` if main Mach-O image could not be identified, and improve reliability of identification.
  [#1097](https://github.com/bugsnag/bugsnag-cocoa/issues/1097)
  [#1101](https://github.com/bugsnag/bugsnag-cocoa/pull/1101)

### Bug fixes

* Fix an issue where the Device.time of an event was missing the milliseconds
  [#298](https://github.com/bugsnag/bugsnag-unity/pull/298)

* Adjust post build script to support Unity 2021 builds
  [#289](https://github.com/bugsnag/bugsnag-unity/pull/289)

* Fix an issue where timestamps and other `:`-containing log message content was interpreted as the error class
  [#292](https://github.com/bugsnag/bugsnag-unity/pull/292)

* Correct Android session start times
  [#291](https://github.com/bugsnag/bugsnag-unity/pull/291)

* Fix duplicate events being sent for Android C/C++ crashes
 
## 5.0.0 (2021-06-08)

This version contains **breaking** changes, as bugsnag-unity has been updated to use the latest available versions of bugsnag-android (v4.22.2 -> v5.9.4) and bugsnag-cocoa (v5.23.5 -> v6.9.3).

Please see the [upgrade guide](./UPGRADING.md) for details of all the changes and instructions on how to upgrade.

### Bug fixes

* Stop scene changes overrriding context when manually set
  [#255](https://github.com/bugsnag/bugsnag-unity/pull/255)

* Don't Destroy TimingTrackerObject, so it persists across scenes
  [#239](https://github.com/bugsnag/bugsnag-unity/pull/239)

## 4.8.8 (2021-04-21)

### Bug fixes

* Add bugsnag prefix to namespace of vendored SimpleJson
  [#225](https://github.com/bugsnag/bugsnag-unity/pull/225)

## 4.8.7 (2021-03-30)

### Bug fixes

* Fix leaks from manual JNI string conversions
  [#222](https://github.com/bugsnag/bugsnag-unity/pull/222)

## 4.8.6 (2021-03-16)

### Bug fixes

* Improve ANR handler compatibility with Google Play reporting mechanism
  [#1179](https://github.com/bugsnag/bugsnag-android/commit/cf905a572296ab1b63af90b24c5d206b4c38b6b4)

## 4.8.5 (2021-03-03)

### Bug fixes

* Avoid JNI crash in leaveBreadcrumb by pushing local frame
  [#214](https://github.com/bugsnag/bugsnag-unity/pull/214)

* Respect autoNotify flag on Android
  [#207](https://github.com/bugsnag/bugsnag-unity/pull/207)

## 4.8.4 (2020-10-05)

### Enhancements

* Add device id to error reports on Cocoa platforms
  [#203](https://github.com/bugsnag/bugsnag-unity/pull/203)

* Update bugsnag-cocoa to v5.23.5:

  * Fix JSON serialisation of strings with control characters
    [#739](https://github.com/bugsnag/bugsnag-cocoa/pull/739)

  * Removed non-thread safe date formatter
    [#758](https://github.com/bugsnag/bugsnag-cocoa/pull/758)

  * Avoid dereference null pointer in JSON serialisation
    [#637](https://github.com/bugsnag/bugsnag-cocoa/pull/637)
    [Naugladur](https://github.com/Naugladur)

  * Fixed an issue where an app could deadlock during a crash if unfavourable
    timing caused DYLD lock contention.
    [#580](https://github.com/bugsnag/bugsnag-cocoa/pull/580)
    [#675](https://github.com/bugsnag/bugsnag-cocoa/pull/675)
    [#725](https://github.com/bugsnag/bugsnag-cocoa/pull/725)
    [#721](https://github.com/bugsnag/bugsnag-cocoa/pull/721)

  * Fix possible report corruption when using `notify()` from multiple threads
    when configured to skip capturing/reporting background thread contents
    (generally only Unity games).
    [#442](https://github.com/bugsnag/bugsnag-cocoa/pull/442)

  * Added several additional event fields (`codeBundleId`, `osName`,
    `modelNumber`, `locale`) that were missing from the OOM reports.
    [#444](https://github.com/bugsnag/bugsnag-cocoa/pull/444)

  * Bugsnag now correctly records a new session if it is returning to
    the foreground after more than 60 seconds in the background.
    [#529](https://github.com/bugsnag/bugsnag-cocoa/pull/529)

## 4.8.3 (2020-06-10)

### Bug fixes

* Delete local JNI references to avoid leaks
  [#198](https://github.com/bugsnag/bugsnag-unity/pull/198)

* Add option to `Configuration` to prevent automatic breadcrumb collection
  [#199](https://github.com/bugsnag/bugsnag-unity/pull/199)

## 4.8.2 (2020-03-31)

### Bug fixes

* Avoid using deprecated AndroidJNI API in Unity 2019
  [#194](https://github.com/bugsnag/bugsnag-unity/pull/194)

## 4.8.1 (2020-02-04)

### Bug fixes

* (Android) Propagate non-string arguments to native android layer correctly
  [#191](https://github.com/bugsnag/bugsnag-unity/pull/191)

## 4.8.0 (2020-01-27)

### Enhancements

This release adds ANR detection for Unity apps running in Android. To enable this option you should
set `Bugsnag.Configuration.AutoDetectAnrs` to `true` after initialising bugsnag in the normal way.

* Detect ANRs on Android and provide configuration option to enable detection
  [#184](https://github.com/bugsnag/bugsnag-unity/pull/184)

### Bug fixes

* (Android) Prevent SIGABRT when altering Configuration on background thread
  [#187](https://github.com/bugsnag/bugsnag-unity/pull/187)

## 4.7.0 (2020-01-22)

* Update bugsnag-android to v4.22.2:

  * This release adds a compile-time dependency on the Kotlin standard library. This should not affect
  the use of any API supplied by bugsnag-unity.

  * Modularise bugsnag-android into Core, NDK, and ANR artifacts
    [#522](https://github.com/bugsnag/bugsnag-android/pull/522)

  * Migrate dependencies to androidx
    [#554](https://github.com/bugsnag/bugsnag-android/pull/554)

  * Report internal SDK errors to bugsnag
    [#570](https://github.com/bugsnag/bugsnag-android/pull/570)
    [#581](https://github.com/bugsnag/bugsnag-android/pull/581)
    [#594](https://github.com/bugsnag/bugsnag-android/pull/594)
    [#605](https://github.com/bugsnag/bugsnag-android/pull/605)
    [#588](https://github.com/bugsnag/bugsnag-android/pull/588)
    [#612](https://github.com/bugsnag/bugsnag-android/pull/612)

  * Add `detectNdkCrashes` configuration option to bugsnag-android to toggle whether C/C++ crashes
    are detected
    [#491](https://github.com/bugsnag/bugsnag-android/pull/491)

  * Use NetworkCallback to monitor connectivity changes on newer API levels
  [#501](https://github.com/bugsnag/bugsnag-android/pull/501)

  * Fix deserialization of custom stackframe fields in cached error reports
    [#576](https://github.com/bugsnag/bugsnag-android/pull/576)

  * Buffer IO when reading from cached error files, improving SDK performance
    [#573](https://github.com/bugsnag/bugsnag-android/pull/573)

  * flushOnLaunch() does not cancel previous requests if they timeout, leading to potential duplicate reports
    [#593](https://github.com/bugsnag/bugsnag-android/pull/593)

  * Report correct value for free disk space
    [#589](https://github.com/bugsnag/bugsnag-android/pull/589)

  * Allow overriding the versionCode via Configuration
    [#610](https://github.com/bugsnag/bugsnag-android/pull/610)

  * Catch throwables when invoking methods on system services
    [#623](https://github.com/bugsnag/bugsnag-android/pull/623)

* Update bugsnag-cocoa to v5.23.0:
  * Fix unrecognized selector crash when adding metadata
    [#430](https://github.com/bugsnag/bugsnag-cocoa/pull/430)

  * This release removes support for reporting 'partial' or 'minimal' crash reports
  where the crash report could not be entirely written (due to disk space or other
  issues like the device battery dying). While sometimes the reports could point
  in the right direction for debugging, they could also be confusing or not enough
  information to pursue and close the issue successfully.

    This release  also renames a few configuration properties to align better with the
  intended use and other Bugsnag libraries, so people who use more than one
  platform can easily find related functionality in a different library. The old
  names are deprecated but still supported until the next major release.
  [#435](https://github.com/bugsnag/bugsnag-cocoa/pull/435)

    * `Bugsnag.setBreadcrumbCapacity()` is now `setMaxBreadcrumbs()` on the
    `BugsnagConfiguration` class. In addition, the default number of breadcrumbs
    saved has been raised to 25 and limited to no more than 100.
    * `BugsnagConfiguration.autoNotify` is now named
    `BugsnagConfiguration.autoDetectErrors`
    * `BugsnagConfiguration.autoCaptureSessions` is now named
    `BugsnagConfiguration.autoDetectSessions`

## 4.6.7 (2019-11-19)

### Bug fixes

* (iOS) Fix a build setting preventing out-of-memory events from being reported
  [#178](https://github.com/bugsnag/bugsnag-unity/pull/178)

## 4.6.6 (2019-10-22)

### Bug fixes

* Fixed the naming and description of the GUI property "Unique logs per second"
  to be "Unique seconds per log" to reflect the actual behavior, which is the
  number of seconds required between unique Unity log messages which
  bugsnag-unity will convert into breadcrumbs or reports. Increase the value to
  reduce the number of logs or reports generated from frequent error messages.
* Update bugsnag-cocoa to v5.22.9:
  * Deprecate `config.reportBackgroundOOMs` property - designating any app
    termination as a possible error condition can cause a lot of false positives,
    especially since the app can die for many genuine reasons, especially when
    running only in the background.
    [bugsnag-cocoa#425](https://github.com/bugsnag/bugsnag-cocoa/pull/425)
  * Fix use-after-free in `notify()` logic which could lead to a deadlock
    [bugsnag-cocoa#420](https://github.com/bugsnag/bugsnag-cocoa/pull/420)
  * Reduce severity of log message about thread status from 'error' to 'debug' as
    it does not necessarily indicate a problem and is only used for debugging.
    [bugsnag-cocoa#421](https://github.com/bugsnag/bugsnag-cocoa/pull/421)
  * Show correct value for `app.inForeground` when an app launches and crashes in
    the background without ever coming to the foreground.
    [bugsnag-cocoa#415](https://github.com/bugsnag/bugsnag-cocoa/pull/415)
  * Fix improperly retained properties which could result in a crash due to
    premature deallocation
    [bugsnag-cocoa#416](https://github.com/bugsnag/bugsnag-cocoa/pull/416)

## 4.6.5 (2019-09-18)

### Bug fixes

* (Android) Fix behavior of `Configuration.NotifyReleaseStages` to disable
  native C/C++ crash reporting if the current `ReleaseStage` is not in the array
* Fix erroneous automatic session delivery when `NotifyReleaseStages`
  configuration does not include the current `ReleaseStage`
* Fix the behavior of `Configuration.AutoNotify` to disable native crash
  reporting if false
* Update bugsnag-cocoa to v5.22.6:
  * Ensure UIKit APIs are not called from background threads if
    `Bugsnag.start()` is called in the background
    [bugsnag-cocoa#409](https://github.com/bugsnag/bugsnag-cocoa/issues/409)
  * Fix bug in `notifyReleaseStages` where if the release stage of a build was
    changed after `start()`, only the initial value was used to determine whether
    to send a report
    [bugsnag-cocoa#405](https://github.com/bugsnag/bugsnag-cocoa/issues/405)
    [bugsnag-cocoa#412](https://github.com/bugsnag/bugsnag-cocoa/issues/412)
  * Support disabling crash reporting after initialization by setting
    `Bugsnag.configuration.autoNotify`. Previously this value was ignored after
    `Bugsnag.start()` was called, but is now used to update whether crash reports
    will be detected and sent. This interface can be used for crash reporting
    opt-out flows.
    [bugsnag-cocoa#410](https://github.com/bugsnag/bugsnag-cocoa/issues/410)

## 4.6.4 (2019-09-06)

### Enhancements

* Support disabling native reporting during initialization
  [#164](https://github.com/bugsnag/bugsnag-unity/pull/164)

  The new API can be used as follows:

  ```c#
  Bugsnag.Init("your-api=key-here", false /* disable crash reporting */);
  ```

  or by unchecking "Auto Notify" when initializing Bugsnag via a GameObject in
  the Unity Inspector.

### Bug fixes

* (Android) Fix possible crash when encoding non-unicode text
  [#165](https://github.com/bugsnag/bugsnag-unity/pull/165)
* (Android) Fix crash when deleting global metadata
  [bugsnag-android#582](https://github.com/bugsnag/bugsnag-android/pull/582)

## 4.6.3 (2019-08-28)

### Bug fixes

* (Android) Fix crash when adding breadcrumbs with null values in metadata
  [bugsnag-android#510](https://github.com/bugsnag/bugsnag-android/pull/510)
* (Android) Fix potential crash when adding a breadcrumb with metadata
  [bugsnag-android#546](https://github.com/bugsnag/bugsnag-android/pull/546)
* (Android) Fix potential crash in when calling `Configuration.setMetaData()`
  [bugsnag-android#513](https://github.com/bugsnag/bugsnag-android/pull/513)
* (Android) Fix potential crash when initializing with ANR detection in low
  connectivity situations
  [bugsnag-android#520](https://github.com/bugsnag/bugsnag-android/pull/520)

## 4.6.2 (2019-08-21)

### Bug fixes

* (Android) Disable reporting for crashes on background threads on x86 devices
  [#161](https://github.com/bugsnag/bugsnag-unity/pull/161)
* (Android) Discard duplicate reports for C/C++ exceptions reporting when Unity
  Cloud Diagnostics is enabled
* (iOS) Update bugsnag-cocoa dependency to v5.22.5:
  * Fix erroneously reporting out-of-memory events from iOS app extensions
    [bugsnag-cocoa#394](https://github.com/bugsnag/bugsnag-cocoa/pull/394)
  * Fix erroneously reporting out-of-memory events when an iOS app is in the
    foreground but inactive
    [bugsnag-cocoa#394](https://github.com/bugsnag/bugsnag-cocoa/pull/394)
  * Fix erroneously reporting out-of-memory events when the app terminates
    normally and is issued a "will terminate" notification, but is terminated
    prior to the out-of-memory watchdog processing the notification
    [bugsnag-cocoa#394](https://github.com/bugsnag/bugsnag-cocoa/pull/394)

## 4.6.1 (2019-07-16)

### Bug fixes

* Update bugsnag-cocoa dependency to v5.22.3
  * Fix JSON parsing errors in crash reports for control characters and some
    other sequences
    [bugsnag-cocoa#382](https://github.com/bugsnag/bugsnag-cocoa/pull/382)
  * Disable reporting out-of-memory events in debug mode, removing false
    positives triggered by killing and relaunching apps using development tools.
    [bugsnag-cocoa#380](https://github.com/bugsnag/bugsnag-cocoa/pull/380)

## 4.6.0 (2019-07-02)

### Enhancements

* Update bugsnag-android dependency to v4.15.0:
  * Make handledState.isUnhandled() publicly readable
    [bugsnag-android#496](https://github.com/bugsnag/bugsnag-android/pull/496)
  * Reduce library size
    [bugsnag-android#492](https://github.com/bugsnag/bugsnag-android/pull/492)

### Bug fixes

* (Android) Fix error class and message parsing for uncaught Java exceptions
  without message contents
  [#159](https://github.com/bugsnag/bugsnag-unity/pull/159)
* Show report metadata added using `Bugsnag.Metadata.Add()` in native crash
  reports
  [#157](https://github.com/bugsnag/bugsnag-unity/pull/157)
* (Android) Fix null pointer dereference when calling Bugsnag.StopSession()
* (Android) Fix abort() in native code when storing breadcrumbs with null values
  in metadata
  [bugsnag-android#511](https://github.com/bugsnag/bugsnag-android/pull/511)
* Update bugsnag-cocoa dependency to v5.22.2:
  * Fix trimming the stacktraces of handled error/exceptions using the
    [`depth`](https://docs.bugsnag.com/platforms/ios/reporting-handled-exceptions/#depth)
    property.
    [Paul Zabelin](https://github.com/paulz)
    [bugsnag-cocoa#363](https://github.com/bugsnag/bugsnag-cocoa/pull/363)
  * Fix crash report parsing logic around arrays of numbers. Metadata which
    included arrays of numbers could previously had missing values.
    [bugsnag-cocoa#365](https://github.com/bugsnag/bugsnag-cocoa/pull/365)

## 4.5.1 (2019-05-28)

### Bug fixes

* Support automatic session tracking when using code-based initialization flow
  rather than a game object and `BugsnagBehaviour`
* Correctly set the "new session" threshold when using automatic session
  tracking to 30 seconds - no more than 1 session should be automatically
  started every 30 seconds when toggling the app between the foreground and
  background.
* Capture initial session on iOS - previously, the `OnApplicationFocus()`
  callback was relied upon to fire the first session, which does not fire at
  launch on iOS (unlike Android). This change starts the first session in the
  frame following BugsnagUnity initialization.
* Updated bugsnag-cocoa to v5.22.1:
  * Report correct app version in out-of-memory reports. Previously the bundle
    version was reported as the version number rather than the short version
    string.
    [bugsnag-cocoa#349](https://github.com/bugsnag/bugsnag-cocoa/pull/349)
  * Fix missing stacktraces in reports generated from `notify()`
    [bugsnag-cocoa#348](https://github.com/bugsnag/bugsnag-cocoa/pull/348)

## 4.5.0 (2019-05-09)

### Enhancements

* Migrate version information in reports to device.runtimeVersions, adding
  additional info like scripting backend
  [#149](https://github.com/bugsnag/bugsnag-unity/pull/149)

* Update bugsnag-android dependency to v4.14.0:
  * Improve In-foreground calculation for report metadata
    [#466](https://github.com/bugsnag/bugsnag-android/pull/466)

  * Migrate version information to device.runtimeVersions
    [#472](https://github.com/bugsnag/bugsnag-android/pull/472)

* Update bugsnag-cocoa dependency to v5.22.0:
  * Migrate version information to device.runtimeVersions
    [#340](https://github.com/bugsnag/bugsnag-cocoa/pull/340)
  * Persist breadcrumbs on disk to allow reading upon next boot in the event of an
    uncatchable app termination.
  * Add `+[Bugsnag appDidCrashLastLaunch]` as a helper to determine if the
    previous launch of the app ended in a crash or otherwise unexpected termination.
  * Report unexpected app terminations on iOS as likely out of memory events where
    the operating system killed the app
  * Add configuration option (`reportOOMs`) to disable out-of-memory (OOM) event
    reporting, defaulting to enabled.
    [#345](https://github.com/bugsnag/bugsnag-cocoa/pull/345)
  * Disable background OOM reporting by default. It can be enabled using
    `reportBackgroundOOMs`.
    [#345](https://github.com/bugsnag/bugsnag-cocoa/pull/345)

### Bug fixes

* Update bugsnag-android dependency to v4.14.0:
  * [NDK] Fix possible null pointer dereference
  * [NDK] Fix possible memory leak if bugsnag-android-ndk fails to successfully
    parse a cached crash report
  * [NDK] Fix possible memory leak when using `bugsnag_leave_breadcrumb()` or
    `bugsnag_notify()`

## 4.4.1 (2019-05-07)

### Bug fixes

* Fix failure to include session info in native reports after stopping/resuming
  a session
* (iOS) Fix session start date formatting
* Support `Notify()` on background threads by caching values from
  `UnityEngine.Application` required for reports during Client initialization
  [#147](https://github.com/bugsnag/bugsnag-unity/pull/147)
* Improve the number of stacktrace frames gathered on Unity 2017.x by
  subscribing to the `logMessageReceived` callback in addition to
  `logMessageReceivedThreaded` and filtering by thread to remove duplicates. The
  main thread-only `logMessageReceived` callback receives more stackframes on
  some versions of Unity 2017, improving the debugging experience.


## 4.4.0 (2019-04-05)

### Enhancements

* Update bugsnag-android dependency to v4.13.0:
  * Add ANR detection to bugsnag-android (ANR detection is disabled on Unity)
    [bugsnag-android#442](https://github.com/bugsnag/bugsnag-android/pull/442)

  * Add unhandled_events field to native payload
    [bugsnag-android#445](https://github.com/bugsnag/bugsnag-android/pull/445)

  * Add stopSession() and resumeSession() to Client
    [bugsnag-android#429](https://github.com/bugsnag/bugsnag-android/pull/429)

### Bug fixes

* Update bugsnag-android dependency to v4.13.0:
  * Prevent overwriting config.projectPackages if already set
    [bugsnag-android#428](https://github.com/bugsnag/bugsnag-android/pull/428)

  * Fix incorrect session handledCount when notifying in quick succession
    [bugsnag-android#434](https://github.com/bugsnag/bugsnag-android/pull/434)

  * Ensure boolean object from map serialised as boolean primitive in JNI
    [bugsnag-android#452](https://github.com/bugsnag/bugsnag-android/pull/452)

  * Prevent NPE occurring when calling resumeSession()
    [bugsnag-android#444](https://github.com/bugsnag/bugsnag-android/pull/444)

* Update bugsnag-cocoa dependency to v5.19.1:
  * Fix generating an incorrect stacktrace used when logging an exception to
    Bugsnag from a location other than the original call site (for example, from a
    logging function or across threads).  If an exception was raised/thrown, then
    the resulting Bugsnag report from `notify()` will now use the `NSException`
    instance's call stack addresses to construct the stacktrace, ignoring depth.
    This fixes an issue in macOS exception reporting where `reportException` is
    reporting the handler code stacktrace rather than the reported exception
    stack.
    [bugsnag-cocoa#334](https://github.com/bugsnag/bugsnag-cocoa/pull/334)

  * Fix network connectivity monitor by connecting to the correct domain
    [Jacky Wijaya](https://github.com/Jekiwijaya)
    [bugsnag-cocoa#332](https://github.com/bugsnag/bugsnag-cocoa/pull/332)

## 4.3.0 (2019-03-14)

This release is the first to distribute Unity packages with and without 64-bit
ABI libraries for Android. In most cases, `Bugsnag.unitypackage` is the correct
package to use, as by default most Unity Android apps only can use 32-bit
binaries.

### Enhancements

* Add StopSession() and ResumeSession() to public API
  [#136](https://github.com/bugsnag/bugsnag-unity/pull/136)

* Update bugsnag-android dependency to v4.11.0:
  * Add stopSession() and resumeSession() to Client
    [bugsnag-android#429](https://github.com/bugsnag/bugsnag-android/pull/429)
  * Prevent overwriting config.projectPackages if already set
    [bugsnag-android#428](https://github.com/bugsnag/bugsnag-android/pull/428)
  * Fix incorrect session handledCount when notifying in quick succession
    [bugsnag-android#434](https://github.com/bugsnag/bugsnag-android/pull/434)

* Update bugsnag-cocoa dependency to v5.19.0:
  * Add stopSession() and resumeSession() to Bugsnag
    [bugsnag-cocoa#325](https://github.com/bugsnag/bugsnag-cocoa/pull/325)
  * Capture basic report diagnostics in the file path in case of crash report
    content corruption
    [bugsnag-cocoa#327](https://github.com/bugsnag/bugsnag-cocoa/pull/327)

### Bug fixes

* Ensure session and user information is included in native crash reports
  [#138](https://github.com/bugsnag/bugsnag-unity/pull/138)
  [bugsnag-cocoa#333](https://github.com/bugsnag/bugsnag-cocoa/pull/333)
  [bugsnag-android#439](https://github.com/bugsnag/bugsnag-android/pull/439)

* [Android] Remove references to 64-bit ABIs included in the package
  [#139](https://github.com/bugsnag/bugsnag-unity/pull/139)

* Make `ErrorClass` and `ErrorMessage` mutable on `Exception`. This allows for
  modifying both properties from callbacks:

  ```cs
  Bugsnag.Notify(exception, report => {
    report.Exceptions[0].ErrorClass = "CustomException";
    report.Exceptions[0].ErrorMessage = "something notable";
  });
  ```
  [#140](https://github.com/bugsnag/bugsnag-unity/pull/140)

* Set severity reason when manually specifying severity as a parameter to
  `Notify()`. This sets the correct reason for the severity selection on the
  dashboard.
  [#140](https://github.com/bugsnag/bugsnag-unity/pull/140)

## 4.2.2 (2019-02-26)

### Bug fixes

* (Android) Fix a crash which could occur if large amounts of metadata or
  breadcrumbs (>500 key/value pairs) are attached at once, or if `Notify()` is
  called concurrently several times.
  [#132](https://github.com/bugsnag/bugsnag-unity/pull/132)

## 4.2.1 (2019-02-21)

### Bug fixes

* Ensure a stacktrace is attached to reports without underlying exceptions, such
  as log messages using an 'error' or 'warning' level
  [#131](https://github.com/bugsnag/bugsnag-unity/pull/131)

## 4.2.0 (2019-02-07)

### Enhancements

* Add support for detecting and reporting Android NDK C/C++ crashes
  [#127](https://github.com/bugsnag/bugsnag-unity/pull/127)

## 4.1.3 (2019-02-01)

### Bug fixes

* Ensure configuration options are synced with native layer
  [#124](https://github.com/bugsnag/bugsnag-unity/pull/124)

* Add retry delays after delivery failure to stop excess logging
  [#126](https://github.com/bugsnag/bugsnag-unity/pull/126)

## 4.1.2 (2019-01-22)

### Bug fixes

Update bugsnag-android dependency to v4.11.0:
* Fix cached error deserialisation where the Throwable has a cause
  [#418](https://github.com/bugsnag/bugsnag-android/pull/418)
* Refactor error report deserialisation
  [#419](https://github.com/bugsnag/bugsnag-android/pull/419)
* Fix unlikely initialization failure if a device orientation event listener
  cannot be enabled
* Cache result of device root check
  [#411](https://github.com/bugsnag/bugsnag-android/pull/411)
* Prevent unnecessary free disk calculations on initialisation
 [#409](https://github.com/bugsnag/bugsnag-android/pull/409)

## 4.1.1 (2019-01-21)

### Bug fixes

* Lower minimum supported iOS version to 8.0 from 9.0
* Lower minimum supported macOS version to 10.8 from 10.11

## 4.1.0 (2019-01-11)

* Update dependent libraries:
  * bugsnag-cocoa v5.17.3
  * bugsnag-android v4.10.0

### Enhancements

* Add a configuration option for allowing Unity exceptions to reduce a project's
  stability score. [See the documentation for
  `ReportUncaughtExceptionsAsHandled`](https://docs.bugsnag.com/platforms/unity/configuration-options/#ReportUncaughtExceptionsAsHandled)

### Bug fixes

* Fix exception parsing for Android Java exceptions to ensure correct grouping,
  "handled"-ness, and remove extraneous stack frame from the top of the
  backtrace.
* Fix off-by-one error in event counts in the session tracking implementation

## 4.0.0 (2018-11-19)

This is a new major release with many features and fixes. See the [upgrade
guide](https://github.com/bugsnag/bugsnag-unity/blob/master/UPGRADING.md) for
more information on how to update your integration, or the [integration
guide](https://docs.bugsnag.com/platforms/unity/) to get started.

### Enhancements

* Unity/C# errors are now automatically detected and reported on Windows and any
  other Unity platform (not including native crashes)
* Exceptions logged on background threads via the [Unity `Debug.Log` API]() are
  now automatically detected and reported in additional to exceptions detected
  on the main thread.
* Support for [session and stability tracking](https://docs.bugsnag.com/product/releases/releases-dashboard/#stability-score)
* Support for [logging detailed breadcrumbs](https://docs.bugsnag.com/platforms/unity/#logging-breadcrumbs)

## 3.6.7 (2018-09-21)

### Improvements

* Automatically detect app version from Unity environment for WebGL
  | [#94](https://github.com/bugsnag/bugsnag-unity/pull/94)
* Report app information in WebGL: `duration`, `inForeground` and `durationInForeground`
  | [#94](https://github.com/bugsnag/bugsnag-unity/pull/94)

**Note:**
During the preparation for this release we noticed a workflow issue when targeting tvOS. In order to successfully build for tvOS, after importing the Bugsnag plugin there is a manual step required:

- In the Unity editor, go to Project > Assets > iOS > Bugsnag
- Select all source files in this directory
- In the inspector pane check `tvOS` in the "Include platforms" list
- Click the "Apply" button at the bottom of the inspector pane

We'll work on making this install process smoother in the future.

## 3.6.6 (2018-05-24)

### Bug fixes

* Support setting the release stage before initializing the client
  | [#73](https://github.com/bugsnag/bugsnag-unity/pull/73)

## 3.6.5 (2018-05-02)

* Upgrade bugsnag-cocoa to v5.15.5:
  - *Bug Fixes:*
    - Changes report generation so that when a minimal or incomplete crash is recorded, essential app/device information is included in the report on the next application launch. [#239](https://github.com/bugsnag/bugsnag-cocoa/pull/239)
  [#250](https://github.com/bugsnag/bugsnag-cocoa/pull/250)
    - Ensure timezone is serialised in report payload.
  [#248](https://github.com/bugsnag/bugsnag-cocoa/pull/248)
  [Jamie Lynch](https://github.com/fractalwrench)

* Upgrade bugsnag-android to v4.3.4:
  - *Bug Fixes:*
    - Avoid adding extra comma separator in JSON if File input is empty or null [#284](https://github.com/bugsnag/bugsnag-android/pull/284)
    - Thread safety fixes to JSON file serialisation [#295](https://github.com/bugsnag/bugsnag-android/pull/295)
    - Prevent potential automatic activity lifecycle breadcrumb crash [#300](https://github.com/bugsnag/bugsnag-android/pull/300)
    - Fix serialisation issue with leading to incorrect dashboard display of breadcrumbs [#306](https://github.com/bugsnag/bugsnag-android/pull/306)
    - Prevent duplicate reports being delivered in low connectivity situations [#270](https://github.com/bugsnag/bugsnag-android/pull/270)
    - Fix possible NPE when reading default metadata filters [#263](https://github.com/bugsnag/bugsnag-android/pull/263)
    - Prevent ConcurrentModificationException in Before notify/breadcrumb callbacks [#266](https://github.com/bugsnag/bugsnag-android/pull/266)
    - Ensure that exception message is never null [#256](https://github.com/bugsnag/bugsnag-android/pull/256)
    - Add payload version to JSON body [#244](https://github.com/bugsnag/bugsnag-android/pull/244)
    - Update context tracking to use lifecycle callbacks rather than ActivityManager [#238](https://github.com/bugsnag/bugsnag-android/pull/238)
  - *Enhancements:*
    - Detect whether running on emulator [#245](https://github.com/bugsnag/bugsnag-android/pull/245)
    - Add a callback for filtering breadcrumbs [#237](https://github.com/bugsnag/bugsnag-android/pull/237)

## 3.6.4 (2018-02-21)

* (iOS) Fix error message displayed when thread tracing disabled

* Update dependent libraries:
  * bugsnag-cocoa v5.15.4
  * bugsnag-android v4.3.1

## 3.6.3 (2018-01-31)

* Prefix WebGL methods

## 3.6.2 (2018-01-18)

* Update dependent libraries:
  * bugsnag-cocoa v5.15.2
  * bugsnag-android v4.3.0

## 3.6.1 (2018-01-09)

* Update dependent libraries:
  * bugsnag-cocoa v5.15.1
  * bugsnag-android v4.2.1

## 3.6.0 (2018-01-08)

### Enhancements

* Support tracking app sessions, which enables showing overall crash rate and
  app health

## 3.5.5 (2017-11-30)
* (iOS) Fix encoding of control characters in crash reports. Ensures crash reports are
written correctly and delivered when containing U+0000 - U+001F

## 3.5.4 (2017-11-23)

* (iOS) Use `BSG_KSCrashReportWriter` header rather than `KSCrashReportWriter` for custom JSON serialization
* (Android) Enqueue activity lifecycle events when initialisation not complete to prevent NPE

## 3.5.3 (2017-11-21)

* Updated Native Cocoa + Android libraries
* Remove misleading Cocoa Error message for non-fatal errors
* Fix handledState cases in Cocoa/Android

## 3.5.2 (2017-11-02)

* Support multiline error messages

## 3.5.1 (2017-10-06)

### Bug fixes

* Fix bundle ambiguity error generated by codesigning the macOS assets
  [#61](https://github.com/bugsnag/bugsnag-unity/pull/61)

## 3.5.0 (2017-02-10)

* Track whether an error is handled or unhandled

* Update native libraries

## 3.4.0 (2017-19-09)

### Enhancements

* Improve error grouping by standardizing log message format
  [#45](https://github.com/bugsnag/bugsnag-unity/pull/45)

* Add breadcrumbs for scene changes
  [Jamie Lynch](https://github.com/fractalwrench)
  [#54](https://github.com/bugsnag/bugsnag-unity/pull/54)

* Load WebGL extension locally, negating the need for a separate request
  [#46](https://github.com/bugsnag/bugsnag-unity/pull/46)

### Bug fixes

* Fix compile error on Unity 5.5
  [#50](https://github.com/bugsnag/bugsnag-unity/issues/50)

## 3.3.2 (24 Apr 2017)

### Bug fixes

* Fix null pointer exception when manually creating the Bugsnag game object
  [Dave Perryman](https://github.com/Pezzah)
  [#41](https://github.com/bugsnag/bugsnag-unity/pull/41)

## 3.3.1 (1 Feb 2017)

### Bug fixes

* Prevents crash for iOS when manually notifying exceptions
  [Ben Ibinson](https://github.com/CodeHex)
  [#40](https://github.com/bugsnag/bugsnag-unity/pull/40)

## 3.3.0 (17 Jan 2017)

### Enhancements

* Adds rate limiting and de-duplication of logs over a set period
  [Ben Ibinson](https://github.com/CodeHex)
  [#39](https://github.com/bugsnag/bugsnag-unity/pull/39)

## 3.2.1 (21 Nov 2016)

### Enhancements

* Add Unity log levels to metadata
  [Ben Ibinson](https://github.com/CodeHex)
  [#38](https://github.com/bugsnag/bugsnag-unity/pull/38)

## 3.2.0 (17 Nov 2016)

### Enhancements

* Provide Init() to support initializing Bugsnag at runtime
  [TimothyKLambert](https://github.com/TimothyKLambert)
  [#34](https://github.com/bugsnag/bugsnag-unity/pull/34)
* Changed default Unity log levels to Bugsnag severity mapping, and provided ability to custom map
  [Ben Ibinson](https://github.com/CodeHex)
  [#36](https://github.com/bugsnag/bugsnag-unity/pull/36)
* Updated KSCrash with iOS performance improvement
  [Ben Ibinson](https://github.com/CodeHex)
  [#37](https://github.com/bugsnag/bugsnag-unity/pull/37)

## 3.1.1 (10 Nov 2016)

### Bug fixes

* Transferred syntax fix on macOS sierra/latest ruby script to package file
  [Ben Ibinson](https://github.com/CodeHex)
  [#35](https://github.com/bugsnag/bugsnag-unity/pull/35)

## 3.1.0 (8 Nov 2016)

### Enhancements

* Add support for tvOS
  [Ben Ibinson](https://github.com/CodeHex)
  [#32](https://github.com/bugsnag/bugsnag-unity/pull/32)

### Bug fixes

* Fixed syntax errors on macOS sierra/latest ruby on post process
  [Delisa Mason](https://github.com/kattrali)
  [#33](https://github.com/bugsnag/bugsnag-unity/pull/33)
* Improve folder structure for iOS and OSX
  [Ben Ibinson](https://github.com/CodeHex)
  [#32](https://github.com/bugsnag/bugsnag-unity/pull/32)
* Compatibility with Unity 4.7
  [Ben Ibinson](https://github.com/CodeHex)
  [#32](https://github.com/bugsnag/bugsnag-unity/pull/32)
* Severity level uses enumeration rather than string
  [Ben Ibinson](https://github.com/CodeHex)
  [#32](https://github.com/bugsnag/bugsnag-unity/pull/32)

## 3.0.3 (16 Sep 2016)

### Bug fixes

* Fixed incorrect reference to then Unity version of the bugsnag-cocoa notifier

## 3.0.2 (8 Sep 2016)

### Bug fixes

* Fix NotifyLevel to be static
  [Ben Ibinson](https://github.com/CodeHex)
  [#29](https://github.com/bugsnag/bugsnag-unity/pull/29)

## 3.0.1 (24 Jun 2016)

#### Fixes

* Includes Android plugin correctly which is now in `aar` format
  | [#24](https://github.com/bugsnag/bugsnag-unity/pull/24)

## 3.0.0

This release includes significant updates to the underlying android and cocoa
libraries.

### Enhancements

* Add support for WebGL
* Add support for OS X
* Add support for setting user metadata
* Add support for setting app version
* Add support for setting breadcrumbs

* Upgrade bugsnag-android from 3.2.5 -> 3.4.0
* Upgrade bugsnag-cocoa from 4.0.7 -> 4.1.0

### Bug Fixes

* Fix crash resulting from use of deprecated method
  [Simon Maynard](https://github.com/snmaynard)
  [#13](https://github.com/bugsnag/bugsnag-unity/pull/13)

* Update callback registration method to remove deprecations
  [Delisa Mason](https://github.com/kattrali)
  [#20](https://github.com/bugsnag/bugsnag-unity/pull/20)

2.2.6
-----

- Fix compilation under arm64

2.2.5
-----

- Allow passing Context as a second argument to Bugsnag.Notify

2.2.4
-----

- Fix use-after-free on NSNotification

2.2.3
-----

- Make context thread safe too

2.2.2
-----

- Make metaData thread safe

2.2.1
-----

- Improve speed during manual notifies on android and ios.

2.2.0
-----

- Fix LogHandler in non-debug builds.
- Make Bugsnag methods static.
- Fix crashes caused by passing unexpected nulls into Bugsnag.

2.1.0
-----

- Fix stacktrace generation in Bugsnag.Notify() on unthrown exceptions.

2.0.0
-----

- Rewrite to use bugsnag-android and bugsnag-ios.
