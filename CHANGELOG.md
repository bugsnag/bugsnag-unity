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
