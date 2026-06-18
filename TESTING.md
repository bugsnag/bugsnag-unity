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
UNITY_VERSION=2021.3.58f1 rake plugin:export
```
This will generate the following files:
* `Bugsnag.unitypackage`
* `Bugsnag-with-android-64bit.unitypackage`

#### Android

To build the Android test fixture:
```
UNITY_VERSION=2021.3.58f1 rake test:android:build
```
Where `UNITY_VERSION` corresponds to the Unity installation path, e.g:
```
/Applications/Unity/Hub/Editor/2021.3.58f1/Unity.app/Contents/MacOS/Unity
```

This will generate a test fixture APK named according to the `UNITY_VERSION`, e.g:
```
./features/fixtures/maze_runner/mazerunner_2021.apk
```

#### iOS

To build the iOS test fixture:
```
UNITY_VERSION=2021.3.58f1 rake test:ios:build
```
Where `UNITY_VERSION` corresponds to the Unity installation path, e.g:
```
/Applications/Unity/Hub/Editor/2021.3.58f1/Unity.app/Contents/MacOS/Unity
```

This will generate a test fixture IPA named according to the `UNITY_VERSION`, e.g:
```
./features/fixtures/maze_runner/mazerunner_2021.ipa
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

1. Run `bundle install` if you haven't run end-to-end tests before
2. Check the contents of `Gemfile` to select the version of `maze-runner` to use
3. To run the tests:
    ```shell script
    bundle exec maze-runner --app=./features/fixtures/maze_runner/mazerunner_2021.apk \
                            --farm=bs                                                        \
                            --device=ANDROID_9
    ```

#### iOS

1. Run `bundle install` if you haven't run end-to-end tests before
2. Check the contents of `Gemfile` to select the version of `maze-runner` to use
3. To run the tests:
    ```shell script
    bundle exec maze-runner --app=./features/fixtures/maze_runner/mazerunner_2021.ipa --farm=bs --device=IOS_26
    ```

## Desktop tests

### Building the test fixture

To build any test fixture, from the root of the repository, first build the notifier:
```
$env:UNITY_VERSION="2021.3.45f2"

rake plugin:export
```
This will generate the following file:
* `Bugsnag.unitypackage`

#### MacOS

`UNITY_VERSION=2021.3.58f1 ./features/scripts/build_maze_runner.sh dev macos`

Where `UNITY_VERSION` corresponds to the Unity installation path, e.g:
```
/Applications/Unity/Hub/Editor/2021.3.58f1/Unity.app/Contents/MacOS/Unity
```

This will generate the test fixture app:
```
./features/fixtures/maze_runner/Mazerunner_dev.app
```

#### Windows

Building the test fixture on Windows requires a Git bash terminal.

In a Git bash terminal:
`UNITY_VERSION=2021.3.45f2 ./features/scripts/build_maze_runner.sh dev windows`

Where `UNITY_VERSION` corresponds to the Unity installation path, e.g:
```
/C:/Program Files/Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity.exe
```

This will generate a build folder containing the test fixture executable, together with the UnityPlayer.dll and other
dependencies:
```
./features/fixtures/maze_runner/build/Windows
```

### Running an end-to-end test

#### MacOS

1. Check the contents of `Gemfile` to select the version of `maze-runner` to use
2. Run `bundle install` if you haven't run end-to-end tests before
3. To run the tests:
    ```shell script
    bundle exec maze-runner --app=features/fixtures/maze_runner/build/MacOS/Mazerunner_dev.app --os=macos
    ```

#### Windows

Running the Maze Runner tests on Windows requires the Ubuntu app (using WSL).

In the Ubuntu terminal:
1. Check the contents of `Gemfile` to select the version of `maze-runner` to use
2. Run `bundle install` if you haven't run end-to-end tests before
3. To run the tests:
    ```shell script
    bundle exec maze-runner --app=features/fixtures/maze_runner/build/Windows/Mazerunner_dev.exe --os=windows
    ```

#### WebGL

The WebGL e2e tests depend on Chrome and `chromedriver` (available from Homebrew).
1. Building the test fixture on WebGL
UNITY_VERSION=2021.3.58f1 ./features/scripts/build_maze_runner.sh dev webgl
2. Check the contents of `Gemfile` to select the version of `maze-runner` to use
3. Run `bundle install` if you haven't run end-to-end tests before
4. To run the tests:
    ```shell script
    
    bundle exec maze-runner --farm=local --browser=chrome