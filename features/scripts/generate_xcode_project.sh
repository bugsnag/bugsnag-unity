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

DEFAULT_CLI_ARGS="-quit -nographics -batchmode -logFile unity.log"

project_path=`pwd`/maze_runner

if [ "$BUILD_TYPE" == "dev" ]; then
  BUILD_METHOD="Builder.IosDev"
elif [ "$BUILD_TYPE" == "release" ]; then
  BUILD_METHOD="Builder.IosRelease"
else
  echo "Invalid build type specified: 'release' or 'dev' only"
  exit 1
fi

# Generate the Xcode project for iOS
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod $BUILD_METHOD
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

# Unity needs an -ObjC linker flag but using the undocumented SetAdditionalIl2CppArgs method appears to have no effect
# a clean build. Building the project twice is a temporary workaround that circumnavigates what
# appears to be a platform bug.
# For further context see https://answers.unity.com/questions/1610105/how-to-add-compiler-or-linker-flags-for-il2cpp-inv.html
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod $BUILD_METHOD
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi
