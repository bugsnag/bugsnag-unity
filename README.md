Bugsnag Notifier for Unity
============================

The Bugsnag Notifier for Unity gives you instant notification of exceptions
thrown from your Unity applications on iOS and Android devices. 
The notifier automatically notifies about uncaught exceptions in C#, as well as native code.

[Bugsnag](http://bugsnag.com) captures errors in real-time from your web, 
mobile and desktop applications, helping you to understand and resolve them 
as fast as possible. [Create a free account](http://bugsnag.com) to start 
capturing exceptions from your applications.


Installation & Setup
--------------------

Install the Bugsnag notifier from within the Asset Store [here]().

Alternatively you can download the unitypackage file from within our [Github repository](https://github.com/bugsnag/bugsnag-unity/raw/master/bugsnag.unitypackage). Once the package has downloaded, double click it to import the Bugsnag notifier into your Unity project.

Then select a GameObject within your first scene. This scene should be the first scene loaded when starting your game, i.e. the loading scene for example. With a GameObject selected, click "Add Component" within the inspector in the Unity editor. Then select Scripts and then Bugsnag from the dropdown.

Finally you should configure your ApiKey by selecting the GameObject in the Unity editor and setting the ApiKey in the inspector.


iOS Specific Setup
------------------

When building for iOS, in order to be notified of uncaught exceptions in Unity, the player setting "Script Call Optimization" setting should be set to "Slow and Safe". This may affect your game performance so you may not wish to do this on production. It is worth noting that this step is not required on Android, so uncaught Unity exceptions will still be reported from your Android clients.

You also need to enable the "Enable Objective-C Exceptions" build setting in your Xcode project.


Send Non-Fatal Exceptions to Bugsnag
------------------------------------

If you would like to send non-fatal exceptions to Bugsnag, you can pass any
`Throwable` object to the `notify` method:

```csharp
Bugsnag.Notify(new System.InvalidOperationException("Non-fatal"));
```

Adding Tabs to Bugsnag Error Reports
------------------------------------

If you want to add a tab to your Bugsnag error report, you can call the `addToTab` method:

```csharp
Bugsnag.AddToTab("user", "username", "bob-hoskins");
Bugsnag.AddToTab("user", "registered_user", "yes");
```

This will add a user tab to the error report on bugsnag.com that contains the username and whether the user was registered or not.

You can clear a single attribute on a tab by calling:

```csharp
Bugsnag.AddToTab("user", "username", null);
```

or you can clear the entire tab:

```csharp
Bugsnag.ClearTab("user");
```

Configuration
-------------

###Context

Bugsnag uses the concept of "contexts" to help display and group your
errors. Contexts represent what was happening in your application at the
time an error occurs. In a Unity game, it is useful to set this to be 
your currently active `Scene`.

If you would like to set the bugsnag context manually, you can set the 
`Context` property:

```csharp
Bugsnag.Context = "MyActivity";
```

###UserId

Bugsnag helps you understand how many of your users are affected by each
error. In order to do this, we send along a userId with every exception. 
By default we will generate a unique ID and send this ID along with every 
exception from an individual device.
    
If you would like to override this `userId`, for example to set it to be a
username of your currently logged in user, you can set the `UserId` property:

```csharp
Bugsnag.UserId = "leeroy-jenkins";
```

###ReleaseStage

If you would like to distinguish between errors that happen in different
stages of the application release process (development, production, etc)
you can set the `ReleaseStage` that is reported to Bugsnag.

```csharp
Bugsnag.ReleaseStage = "development";
```
    
By default this is set to be "production" if the build is a release build, and "development" for a development build.

###AutoNotify

By default, we will automatically notify Bugsnag of any fatal exceptions
in your application. If you want to stop this from happening, you can set the 
`AutoNotify` property:
    
```csharp
Bugsnag.AutoNotify = false;
```

You can also set this as a property on the attached GameObject within the Unity editor.

###NotifyLevel

By default, bugsnag will be notified about Exceptions that are logged to Debug.LogException. You can lower the notify level to Error if you want to notify bugsnag of all Debug.LogError calls as well by setting the NotifyLevel property:

```csharp
Bugsnag.NotifyLevel = LogSeverity.Error;
```

You can also set this as a property on the attached GameObject within the Unity editor.

Building from Source
--------------------

To build a `.unitypackage` file from source, clone the [bugsnag-android](https://github.com/bugsnag/bugsnag-android) repository, the [bugsnag-ios](https://github.com/bugsnag/bugsnag-ios) repository and the [bugsnag-unity](https://github.com/bugsnag/bugsnag-unity) repository. Then, within the bugsnag-unity repository, run:

```bash
./build.sh
```

This will generate a file named `build/bugsnag.unitypackage`.


Reporting Bugs or Feature Requests
----------------------------------

Please report any bugs or feature requests on the github issues page for this
project here:

<https://github.com/bugsnag/bugsnag-unity/issues>


Contributing
------------

-   [Fork](https://help.github.com/articles/fork-a-repo) the [notifier on github](https://github.com/bugsnag/bugsnag-unity)
-   Commit and push until you are happy with your contribution
-   [Make a pull request](https://help.github.com/articles/using-pull-requests)
-   Thanks!


License
-------

The Bugsnag Unity notifier is free software released under the MIT License. 
See [LICENSE.txt](https://github.com/bugsnag/bugsnag-unity/blob/master/LICENSE.txt) for details.