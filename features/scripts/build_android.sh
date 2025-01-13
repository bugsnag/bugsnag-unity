#!/usr/bin/env bash

if [ -z "$UNITY_VERSION" ]
then
  echo "UNITY_VERSION must be set"
  exit 1
fi

if [ -z "$1" ]
then
  echo "Build type must be specified: 'release' or 'dev'"
  exit 1
fi

BUILD_TYPE=$1

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS"

pushd "${0%/*}"
  script_path=`pwd`
popd

pushd "$script_path/../fixtures"

# Run unity and immediately exit afterwards, log all output
DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile build_android_apk.log -buildTarget Android"

project_path=`pwd`/maze_runner

if [ "$BUILD_TYPE" == "dev" ]; then
  BUILD_METHOD="Builder.AndroidDev"
  OUTPUT_APK="mazerunner_dev.apk"
  RENAMED_APK="mazerunner_dev_${UNITY_VERSION:0:4}.apk"
elif [ "$BUILD_TYPE" == "release" ]; then
  BUILD_METHOD="Builder.AndroidRelease"
  OUTPUT_APK="mazerunner.apk"
  RENAMED_APK="mazerunner_${UNITY_VERSION:0:4}.apk"
else
  echo "Invalid build type specified: 'release' or 'dev' only"
  exit 1
fi

# Build for Android
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod $BUILD_METHOD
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

mv $project_path/$OUTPUT_APK $project_path/$RENAMED_APK
