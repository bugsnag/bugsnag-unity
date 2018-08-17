
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

- Install Unity
- Set up your Xcode (requires being a member of the Apple Developer Program)
- Set up the Android SDK (using [instructions](https://github.com/bugsnag/bugsnag-android/blob/master/CONTRIBUTING.md) from bugsnag-android)
- Open the example app in Unity
- You can build the app for iPhone or Android using the custom Build menu.

## Installing and using multiple versions of Unity

You can install as many versions of Unity as you like on the same computer. On a Mac the installer creates a folder called Unity, and overwrites any existing folder with this name. If you want more than one version of Unity on your Mac, rename the existing Unity folder before installing another version. On a PC, the install folder defaults to C:\Program Files\Unity, this can be changed to another path so that you can install more than one version.

The build script will by default locate Unity in it's default location on both Mac and Windows machines. If you want to use an alternative location for Unity (to test against multiple versions for instance) then you can specify the location in an ENV variable when running the build script.

MacOS
```
UNITY_DIR=/Applications/Unity.2018.2.3 bundle exec rake
```

Windows
```
$env:UNITY_DIR="C:\Program Files\Unity.2018.2.3\"
bundle exec rake
```

## Testing Changes

A simple project can be found at [example](https://github.com/bugsnag/bugsnag-unity/blob/master/example), which allows various crashes to be triggered by clicking buttons.

## Building Plugin

The plugin can be built by running

```
bundle install
bundle exec rake
```

## Building Example

```
bundle install
bundle exec rake example:build:all
```

## Releasing a new version

### Release Checklist

#### Pre-release

- [ ] Are all changes committed?
- [ ] Does the build pass on the CI server?
- [ ] Has the changelog been updated?
- [ ] Have all the version numbers been incremented? Update the version number by running `make VERSION=[number] bump`.
- [ ] Has all new functionality been manually tested on a release build? Use `rake build` to generate an artifact to install in a new app.
  - [ ] Is development mode disabled? Disable development mode in the Unity Build dialog when testing release builds.
  - [ ] Test that a log message formatted as `SomeTitle: rest of message` generates an error titled `SomeTitle` with message `rest of message`
  - [ ] Test that a log message formatted without a colon generates an error titled `LogError<level>` with message `rest of message`
  - [ ] Ensure the example app sends the correct error for each type on iOS
  - [ ] Ensure the example app sends the correct error for each type on tvOS
  - [ ] Ensure the example app sends the correct error for each type on macOS
  - [ ] Ensure the example app sends the correct error for each type on Android
  - [ ] Ensure the example app sends the correct error for each type on WebGL
  - [ ] Archive the iOS app and validate the bundle type
  - [ ] Generate a signed APK for Android
- [ ] Do the installation instructions work when creating an example app from scratch?
- [ ] Are PRs open on the docs site for any new feature changes or version numbers?
- [ ] Have the installation instructions been updated on the [dashboard](https://github.com/bugsnag/bugsnag-website/tree/master/app/views/dashboard/projects/install)
- [ ] Have the installation instructions been updated on the [docs site](https://github.com/bugsnag/docs.bugsnag.com)?


#### Making the release

1. Commit the changelog and version updates:

    ```
    git add CHANGELOG.md src/BugsnagUnity.mm bugsnag-android-unity/src/main/java/com/bugsnag/android/unity/UnityCallback.java
    git commit -m "Release v3.x.x"
    git tag "v3.x.x"
    git push origin master --tags
    ```
2. [Create a new release on GitHub](https://github.com/bugsnag/bugsnag-unity/releases/new), copying the changelog entry.
    * set the title to the tag name
    * upload `Bugsnag.unitypackage`

#### Post-release

- [ ] Have all Docs PRs been merged?
- [ ] Can the latest release be installed by downloading the artifact from the releases page?
- [ ] Do the installation instructions on the dashboard work using the released artefact?
- [ ] Do the installation instructions on the docs site work using the released artefact?
- [ ] Can a freshly created example app send an error report from a release build, using the released artefact?
- [ ] Do the existing example apps send an error report using the released artefact?
