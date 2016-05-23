Bugsnag Notifier for Unity
============================

The Bugsnag Notifier for Unity gives you instant notification of exceptions
thrown from your Unity games on iOS and Android devices, as well as standalone
Mac and WebGL deployments. Exceptions in your Unity code (JS, C# and Boo) as
well as native crashes (Objective C, Java) are automatically detected and sent to Bugsnag.

[Bugsnag](http://bugsnag.com) captures errors in real-time from your web
and mobile apps, helping you to understand and resolve them as fast as possible.
[Create a free account](http://bugsnag.com) to start capturing exceptions today.


Basic Installation & Setup
--------------------------

1.  **Download & Import the Bugsnag Unity Package**

    Download the latest [Bugsnag Unity Notifier](https://github.com/bugsnag/bugsnag-unity/raw/master/Bugsnag.unitypackage).
    Double click the `.unitypackage` file, then click the *import* button to
    import Bugsnag into your Unity project.

    ![Import Package](https://raw.github.com/bugsnag/bugsnag-unity/master/assets/docs/import-package.png)

2.  **Create a Bugsnag GameObject**

    Create a new `GameObject` within your first/main scene
    (GameObject Menu -> Create Empty), and rename it to *Bugsnag*.
    
    ![Create GameObject](https://raw.github.com/bugsnag/bugsnag-unity/master/assets/docs/create-gameobject.png)

3.  **Add the Bugsnag Component to Your GameObject**

    With your new GameObject selected, add the Bugsnag component 
    (Component Menu -> Scripts -> Bugsnag). 
    
    ![Add Bugsnag Component](https://raw.github.com/bugsnag/bugsnag-unity/master/assets/docs/add-bugsnag-component.png)

4.  **Configure Your API Key**

    Finally, configure your Bugsnag API Key in the component inspector.
    You can find your API key on the *Project Settings* page on your 
    [Bugsnag dashboard](https://bugsnag.com).

    ![Set API Key](https://raw.github.com/bugsnag/bugsnag-unity/master/assets/docs/set-api-key.png)

Additional Setup (Android)
------------------------------

When building for Android, you'll need to make sure that Internet Access is 
enabled. On the *Android* tab of the Unity *Player Settings* screen, make sure
*Internet Acess* is set to *Require*.

![Enable Internet Access](https://raw.github.com/bugsnag/bugsnag-unity/master/assets/docs/android-internet-access.png)

How to Send Non-Fatal Exceptions to Bugsnag
-------------------------------------------

If you would like to send non-fatal exceptions to Bugsnag, you can pass any
`Throwable` object to Bugsnag's `notify` method in your Unity scripts:

```csharp
Bugsnag.Notify(new System.InvalidOperationException("Non-fatal"));
```

Alternatively, you can change the Bugsnag [NotifyLevel](#notifylevel) setting
(see below) to send `Debug.Log*` events to Bugsnag.


How to Add Custom Data to Bugsnag Error Reports
-----------------------------------------------

If you would like to add custom data to your Bugsnag error reports, you can
use the `AddToTab` method. Custom data is displayed inside tabs on each 
exception on your Bugsnag dashboard.

```csharp
Bugsnag.AddToTab("user", "username", "bob-hoskins");
Bugsnag.AddToTab("user", "registered_user", "yes");
```

This will add a *user* tab to the error report on your Bugsnag dashboard
containing the username and registration status of your user.


Optional Configuration
----------------------

Most Bugsnag settings can be configured from the Unity Inspector, but you can
also change settings inside your scripts:

###AutoNotify

By default, we will automatically notify Bugsnag of any fatal exceptions
(crashes) in your game. If you want to stop this from happening, you can set
the *Auto Notify* property in the Unity Inspector.

You can also change this setting in your scripts as follows:
    
```csharp
Bugsnag.AutoNotify = false;
```

###NotifyLevel

By default, Bugsnag will be notified about any Exception logged to 
`Debug.LogException`. You can send additional log information to Bugsnag 
(eg. all `Debug.LogError` messages) by changing the *Notify Level* property
in the Unity Inspector.

You can also change this setting in your scripts as follows:

```csharp
Bugsnag.NotifyLevel = LogSeverity.Error;
```

###NotifyUrl

By default Bugsnag sends exceptions to bugsnag.com, if you're running
[Bugsnag Enterprise](https://bugsnag.com/enterprise), you can change
this to point to your local Bugsnag server:

```csharp
Bugsnag.NotifyUrl = 'http://bugsnag.internal.example.com';
```

###Context

Bugsnag uses the concept of *contexts* to help display and group your
errors. Contexts represent what was happening in your game at the
time an error occurs. By default, this will be set to be your currently
active Unity Scene.

If you would like to set the Bugsnag context manually, you can set the 
`Context` property in your scripts:

```csharp
Bugsnag.Context = "Space Port";
```

###ReleaseStage

If you would like to distinguish between errors that happen in different
stages of your game's release process (development, qa, production, etc)
you can set the `ReleaseStage` in your scripts:

```csharp
Bugsnag.ReleaseStage = "development";
```

By default this is set to be *production* if the build is a release build,
and *development* for a development build.

###NotifyReleaseStages

By default, we notify Bugsnag of all exceptions that happen in your app. If you would like to change which release stages notify Bugsnag of exceptions you can set the notifyReleaseStages property:

```csharp
Bugsnag.NotifyReleaseStages = ["staging", "production"];
```

###AppVersion

If you want to manually track in which versions of your application each exception happens, you can set `AppVersion`.

```csharp
Bugsnag.AppVersion = "1.2.3-alpha";
```

###Logging breadcrumbs

You can add custom log messages called "breadcrumbs" to document what user interactions occurred in your application prior to a crash. Each breadcrumb also records the time at which it was left. To record a breadcrumb:

```csharp
Bugsnag.LeaveBreadcrumb("Player won");
Bugsnag.LeaveBreadcrumb("Button was clicked");
```

By default, we'll store and send the last 20 breadcrumbs you leave before errors are sent to Bugsnag. If you'd like to increase this number, you can call `SetBreadcrumbCapacity`

```csharp
Bugsnag.BreadcrumbCapacity = 50;
```
Please note that breadcrumbs are not supported for WebGL deployments.

###User

Bugsnag helps you understand how many of your users are affected by each error. In order to do this, we send along a userId with every exception. By default we will generate a unique ID and send this ID along with every exception from an individual device.

If you would like to override this userId, for example to set it to be a username of your currently logged in user, you can set the userId property:

```csharp
Bugsnag.SetUser("userId", "User Name", "user@email.com");
```

You can also set the email and name of the user and these will be searchable in the dashboard.

Building from Source
--------------------

First clone this repository

    git clone --recursive git@github.com:bugsnag/bugsnag-unity

Then run the Rake task:

    rake build

This will generate a `Bugsnag.unitypackage` file which you can drag into your Unity projects.


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
