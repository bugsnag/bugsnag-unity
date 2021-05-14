# Mazerunner tests

E2E tests are implemented with our notifier testing tool [Maze runner](https://github.com/bugsnag/maze-runner), 
which is a black-box test framework written in Ruby.

End to end tests are written in cucumber-style `.feature` files, and need Ruby-backed "steps" in order to know what 
to run. The tests are located in the `features` subdirectories beneath [`test`](/test/).

There are separate sets of tests for the desktop and mobile targets.  

## Mobile tests

### Building the test fixture

Building the mobile test fixtures currently assumes a macOS based Unity installation.  To build any test fixture, 
from the root of the repository, first build the notifier:
```
rake plugin:export
```
This will generate the following files:
* `Bugsnag.unitypackage`
* `Bugsnag-with-android-64bit.unitypackage`

#### Android

To build the Android test fixture:
```
UNITY_VERSION=2018.4.34f1 rake test:android:build
```
Where `UNITY_VERSION` corresponds to the Unity installation path, e.g:
```
/Applications/Unity/Hub/Editor/2018.4.34f1/Unity.app/Contents/MacOS/Unity
```

This will generate a test fixture APK named according to the `UNITY_VERSION`, e.g:
```
./test/mobile/features/fixtures/maze_runner/mazerunner_2018.4.34f1.apk
```

### Running an end-to-end test

__Note: only Bugsnag employees can run the end-to-end tests for mobile targets.__ We have dedicated test infrastructure 
and private BrowserStack credentials that can't be shared outside of the organization.

Remote tests can be run against real devices provided by BrowserStack. In order to run these tests, you need to set 
the following environment variables:

- A BrowserStack App Automate Username: `BROWSER_STACK_USERNAME`
- A BrowserStack App Automate Access Key: `BROWSER_STACK_ACCESS_KEY`
- A path to a [BrowserStack local testing binary](https://www.browserstack.com/local-testing/app-automate): `MAZE_BS_LOCAL`

#### Android 

1. `cd test/mobile`
1. Run `bundle install` if you haven't run end-to-end tests before
1. Check the contents of `Gemfile` to select the version of `maze-runner` to use
1. To run the tests:
    ```shell script
    bundle exec maze-runner --app=./features/fixtures/maze_runner/mazerunner_2018.4.34f1.apk \
                            --farm=bs                                                        \
                            --device=ANDROID_9_0
    ```
