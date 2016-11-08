# Changelog

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
