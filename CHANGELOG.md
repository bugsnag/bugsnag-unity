# Changelog

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
