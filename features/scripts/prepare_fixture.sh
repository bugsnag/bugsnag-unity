#!/usr/bin/env bash

if [ -z "$UNITY_VERSION" ]
then
  echo "UNITY_VERSION must be set"
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS"

pushd "${0%/*}"
  script_path=`pwd`
popd

pushd "$script_path/../fixtures"

# Assemble the Android AAR and copy it into the Unity plugins directory
echo "Assembling Android mazerunner code into AAR"
./maze_runner/nativeplugin/android/gradlew -p maze_runner/nativeplugin/android assembleRelease

RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

echo "Copying Android mazerunner AAR into Unity plugins dir"
cp maze_runner/nativeplugin/android/build/outputs/aar/android-release.aar maze_runner/Assets/Plugins/Android/mazerunner_code.aar

RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

# Run unity and immediately exit afterwards, log all output, disable the
# package manager (we just don't need it and it slows things down)
DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile unity.log"
project_path=`pwd`/maze_runner

# Installing the Bugsnag package
echo "Importing Bugsnag.unitypackage into $project_path"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS \
                  -projectPath $project_path \
                  -ignoreCompilerErrors \
                  -importPackage $script_path/../../Bugsnag.unitypackage
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

cd maze_runner/packages
rm -rf performance-package
git clone https://github.com/bugsnag/bugsnag-unity-performance-upm.git performance-package


# Open the Unity fixture
echo "Opening Unity fixture..."
$UNITY_PATH/Unity -projectPath $project_path &

UNITY_PID=$!
echo "Unity running with PID $UNITY_PID"

# Wait for 10 seconds
sleep 40

# Close the Unity fixture
echo "Closing Unity fixture..."
kill $UNITY_PID