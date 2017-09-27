
# Contributing

- [Fork](https://help.github.com/articles/fork-a-repo) the [notifier on github](https://github.com/bugsnag/bugsnag-android)
- Build and test your changes
- Commit and push until you are happy with your contribution
- [Make a pull request](https://help.github.com/articles/using-pull-requests)
- Thanks!

## Set up a development environment

- Clone the repo including submodules

    ```
    git clone --recursive git@github.com:bugsnag/bugsnag-unity
    ```

- Set up your Xcode (requires being a member of the Apple Developer Program)
- Set up the Android SDK (using [instructions](https://github.com/bugsnag/bugsnag-android/blob/master/CONTRIBUTING.md) from bugsnag-android)
- Open the example app in Unity
- You can build the app for iPhone or Android using the custom Build menu.

## Testing Changes
A simple project can be found at [examples/Assets/Buttons.unity](https://github.com/bugsnag/bugsnag-unity/blob/master/example/Assets/Buttons.unity), which allows various crashes to be triggered by clicking buttons.

## Upgrading bugsnag-cocoa/bugsnag-android

- Update the submodule

    ```
    cd bugsnag-cocoa; git pull origin unity; cd ..
    cd bugsnag-android; git pull origin master; cd ..
    cd ..; git commit -am "updating notifiers"
    ```

- Update the plugins in the example app

    ```
    rake update_example_plugins
    ```

- Build and test the example app

    ```
    rake build:ios
    rake build:android
    ```

## Modifying the C# code

- Make changes to src/Assets
- Copy changes into the example app

    ```
    rake update
    ```

- Build and test the example app

    ```
    rake build:ios
    rake build:android
    ```


## Releasing a new version

## Release Checklist
Please follow the testing instructions in [the platforms release checklist](https://github.com/bugsnag/platforms-release-checklist/blob/master/README.md), and any additional steps directly below.

- Disable development mode in the Unity Build dialog when testing release builds.

### Instructions

- Build the package, ensuring it can be run correctly:

    ```
    rake build
    ```

- Update the CHANGELOG
- Update the version number in `src/BugsnagUnity.mm` and
  `android/src/main/java/com/bugsnag/android/unity/UnityCallback.java`
- Commit the changelog and version updates:

    ```
    git commit -am "v2.x.x"
    git tag "v2.x.x"
    git push origin master --tags
    ```
- Add a new release on Github

    - https://github.com/bugsnag/bugsnag-unity/releases/new?tag=v2.x.x
    - set the title to the tag name
    - copy the CHANGELOG entry to the release notes
    - upload `Bugsnag.unitypackage`

## Update docs.bugsnag.com

Update the setup guide for Unity with any new content.

