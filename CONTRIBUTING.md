
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

There is a helper script that will use homebrew cask to install a version of 5.6, 2017 and 2018 which are the three major versions of Unity that we support. Along with the support packages for iOS, tvOS and Android. This script only works on MacOS.

MacOS
```
scripts/bootstap-unity.sh
```

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

The plugin can be built for release by running `rake plugin:export`. The
`export` task does a full clean build.

```
bundle install
bundle exec rake plugin:export
```

The plugin can be built with a cache using `rake plugin:quick_export`.

```
bundle exec rake plugin:rebuild
```

List available tasks using `rake -T`.

## Building Example

```
bundle install
bundle exec rake example:build:all
```

## Running maze-runner

We have a very simple maze-runner setup, which builds a MacOS X Unity game that sends a simple notification.

NOTE: This does not currently run on Windows

```
bundle install
bundle exec maze-runner features/handled_errors.feature
```

## Releasing a new version

### Release Checklist

#### Pre-release

- [ ] Has all new functionality been manually tested on a release build? Disable development mode in the Unity Build dialog when testing release builds.
  - [ ] Test that a log message formatted as `SomeTitle: rest of message` generates an error titled `SomeTitle` with message `rest of message`
  - [ ] Test that a log message formatted without a colon generates an error titled `LogError<level>` with message `rest of message`
  - [ ] Ensure the example app sends the correct error for each type on iOS
  - [ ] Ensure OOM reports are sent on iOS when the app is unexpectedly terminated
  - [ ] Ensure the example app sends the correct error for each type on tvOS
  - [ ] Ensure the example app sends the correct error for each type on macOS
  - [ ] Ensure the example app sends the correct error for each type on Android
  - [ ] Ensure the example app sends the correct error for each type on WebGL
  - [ ] Ensure the example app sends the correct error for each type on Windows
  - [ ] Archive the iOS app and validate the bundle type
  - [ ] Archive the Mac app and validate the bundle type
  - [ ] Generate a signed APK for Android
- [ ] Do the installation instructions work when creating an example app from scratch?
- [ ] Are PRs open on the docs site for any new feature changes or version numbers?
- [ ] Have the installation instructions been updated on the [dashboard](https://github.com/bugsnag/bugsnag-website/tree/master/app/views/dashboard/projects/install)
- [ ] Have the installation instructions been updated on the [docs site](https://github.com/bugsnag/docs.bugsnag.com)?


#### Making the release

1. Checkout the `next` branch. Set the version number in the change log and `build.cake`.

2. Commit the changelog and version updates:

    ```
    git add CHANGELOG.md build.cake
    git commit -m "Release v5.x.x"
    ```
3. Make a pull request to merge the changes into `master`

4. Once merged, tag the new release version, pushing the tag to GitHub:

   ```
   git tag v5.x.x
   git push origin v5.x.x
   ```

5. Wait. The CI build will build the new package and create a draft release.

6. Verify that the release looks good, upload the unity packages to the release, copy in the changelog entry into the release notes and publish the draft.

#### Post-release

- [ ] Have all Docs PRs been merged?
- [ ] Can the latest release be installed by downloading the artifacts from the releases page?
- [ ] Do the installation instructions on the dashboard work using the released artifact?
- [ ] Do the installation instructions on the docs site work using the released artifact?
- [ ] Can a freshly created example app send an error report from a release build, using the released artifact?
- [ ] Do the existing example apps send an error report using the released artifact?
