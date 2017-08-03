# Bugsnag exception reporter for Unity
[![Documentation](https://img.shields.io/badge/documentation-latest-blue.svg)](http://docs.bugsnag.com/platforms/unity/)

The Bugsnag Notifier for Unity gives you instant notification of exceptions
thrown from your Unity games on iOS and Android devices, as well as standalone
Mac and WebGL deployments. Exceptions in your Unity code (JS, C# and Boo) as
well as native crashes (Objective C, Java) are automatically detected and sent to Bugsnag.

[Bugsnag](http://bugsnag.com) captures errors in real-time from your web
and mobile apps, helping you to understand and resolve them as fast as possible.
[Create a free account](http://bugsnag.com) to start capturing exceptions today.


## Features

* Automatically report unhandled exceptions and crashes
* Report [handled exceptions](http://docs.bugsnag.com/platforms/unity/#reporting-handled-errors)
* [Log breadcrumbs](http://docs.bugsnag.com/platforms/unity/#logging-breadcrumbs) which are attached to crash reports and add insight to users' actions
* [Attach user information](http://docs.bugsnag.com/platforms/unity/#identifying-users) to determine how many people are affected by a crash


## Getting started

1. [Create a Bugsnag account](https://bugsnag.com)
1. Complete the instructions in the [integration guide](http://docs.bugsnag.com/platforms/unity/) to report unhandled exceptions thrown from your app
1. Report handled exceptions using [`Bugsnag.Notify`](http://docs.bugsnag.com/platforms/unity/#reporting-handled-errors/)
1. Customize your integration using the [configuration options](http://docs.bugsnag.com/platforms/unity/configuration-options/)


## Support

* [Read the integration guide](http://docs.bugsnag.com/platforms/unity/) or [configuration options documentation](http://docs.bugsnag.com/platforms/unity/configuration-options/)
* [Search open and closed issues](https://github.com/bugsnag/bugsnag-unity/issues?utf8=✓&q=is%3Aissue) for similar problems
* [Report a bug or request a feature](https://github.com/bugsnag/bugsnag-unity/issues/new)


## Contributing

All contributors are welcome! For information on how to build, test
and release `bugsnag-unity`, see our
[contributing guide](https://github.com/bugsnag/bugsnag-unity/blob/master/CONTRIBUTING.md).


## License

The Bugsnag Unity notifier is free software released under the MIT License.
See [LICENSE.txt](https://github.com/bugsnag/bugsnag-unity/blob/master/LICENSE.txt)
for details.
