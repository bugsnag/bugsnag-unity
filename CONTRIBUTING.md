
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

## Upgrading bugsnag-cocoa/bugsnag-android

- Update the submodule

    ```
    cd bugsnag-cocoa; git pull origin master; cd ..
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

- Update the CHANGELOG
- Build the package

    ```
    rake build
    ```

- Commit the new build and tag in git
    
    ```
    git commit -am "v2.x.x"
    git tag "v2.x.x"
    git push origin master --tags
    ```
- Add a new release on Github

    - https://github.com/bugsnag/bugsnag-cocoa/releases/new?tag=v2.x.x
    - set the title to the tag name
    - copy the CHANGELOG entry to the release notes
    - upload `Bugsnag.unityframework`


