#!/usr/bin/env bash

if [ -z "$UNITY_PATH" ]
then
  echo "UNITY_PATH must be set, to e.g. /Applications/Unity/Hub/Editor/2017.4.40f1/Unity.app/Contents/MacOS"
  exit 1
fi

echo "\`which Unity\`=`which Unity`"

pushd "${0%/*}"
  script_path=`pwd`
popd

pushd "$script_path/../fixtures"

# Run unity and immediately exit afterwards, log all output, disable the
# package manager (we just don't need it and it slows things down)
DEFAULT_CLI_ARGS="-quit -batchmode -logFile unity.log -noUpm"
project_path=`pwd`/maze_runner

# Creating a new project in the MyProject directory
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -createProject $project_path

# Installing the Bugsnag package
echo "Importing Bugsnag.unitypackage into $project_path"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -importPackage $script_path/../../../../Bugsnag.unitypackage
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

echo "Importing Bugsnag-with-android-64bit.unitypackage into $project_path"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -importPackage $script_path/../../../../Bugsnag-with-android-64bit.unitypackage
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

cp -r Assets/Scenes maze_runner/Assets/
cp -r Assets/Scripts maze_runner/Assets/
cp Assets/Editor/Builder.cs maze_runner/Assets/Scripts/
cp Assets/Plugins/Android/AndroidManifest.xml maze_runner/Assets/Plugins/Android

# Running a custom script - must reference a static method
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod Builder.AndroidBuild
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

mv $project_path/mazerunner.apk $project_path/mazerunner_$UNITY_VERSION.apk
