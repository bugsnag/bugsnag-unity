# Changelog

## TBD

### Bug fixes

* Make `ErrorClass` and `ErrorMessage` mutable on `Exception`. This allows for
  modifying both properties from callbacks:

  ```cs
  Bugsnag.Notify(exception, report => {
    report.Exceptions[0].ErrorClass = "CustomException";
    report.Exceptions[0].ErrorMessage = "something notable";
  });
  ```

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
