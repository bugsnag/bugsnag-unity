
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

You can install as many versions of Unity as you like on the same computer. On a Mac the installer creates a folder 
called Unity, and overwrites any existing folder with this name. If you want more than one version of Unity on your Mac, 
rename the existing Unity folder before installing another version. On a PC, the install folder defaults to 
C:\Program Files\Unity, this can be changed to another path so that you can install more than one version.

There is a helper script that will use homebrew cask to install the major versions of Unity that we support. Along with 
the support packages for iOS, tvOS and Android. This script only works on macOS.

MacOS
```
scripts/bootstap-unity.sh
```

The build script will by default locate Unity in its default location on both Mac and Windows machines. If you want to 
use an alternative location for Unity (to test against multiple versions for instance) then you can specify the location 
in an ENV variable when running the build script.

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

A simple project can be found at [example](https://github.com/bugsnag/bugsnag-unity/blob/master/example), which allows 
various crashes to be triggered by clicking buttons.

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

- [ ] Do the installation instructions work when creating an example app from scratch?
- [ ] Are PRs open on the docs site for any new feature changes or version numbers?
- [ ] Have the installation instructions been updated on the [dashboard](https://github.com/bugsnag/bugsnag-website/tree/master/app/views/dashboard/projects/install)
- [ ] Have the installation instructions been updated on the [docs site](https://github.com/bugsnag/docs.bugsnag.com)?


#### Making the package release

1. Make sure any changes made since last release in `master` are merged into `next`.

2. Checkout the `next` branch.

3. Set the new version in `CHANGELOG.md`

4. Make a pull request to merge the changes into `master`

5. Once merged, tag the new release version, pushing the tag to GitHub using:

   ```
   rake plugin:release
   ```

6. Wait. The CI build will build the new package and create a draft release.

7. Verify that the release looks good, upload the unity packages to the release, copy in the changelog entry into the release notes and publish the draft.

#### Making the UPM release

Once the UnityPackage release is confirmed a UPM release should be deployed

1. Make sure that the package used in the github release is present in the root of the repo.

2. Run:
   ```
   rake plugin:package
   ```

#### Post-release

- [ ] Have all Docs PRs been merged?
- [ ] Can the latest release be installed by downloading the artifacts from the releases page?
- [ ] Do the installation instructions on the dashboard work using the released artifact?
- [ ] Do the installation instructions on the docs site work using the released artifact?
- [ ] Can a freshly created example app send an error report from a release build, using the released artifact?
- [ ] Do the existing example apps send an error report using the released artifact?
