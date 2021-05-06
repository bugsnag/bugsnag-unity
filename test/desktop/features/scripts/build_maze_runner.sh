#!/usr/bin/env bash

export PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS:$PATH"

# Set project_path to the repo root
pushd "${0%/*}"
  pushd ../../../..
    package_path=`pwd`
    echo "Expecting to find Busnag package in: $package_path"
  popd
  pushd ../fixtures
    log_file="$package_path/unity.log"
    project_path="$(pwd)/maze_runner"

    echo "Importing $package_path/Bugsnag.unitypackage into $project_path"
    Unity -nographics -quit -batchmode -logFile $log_file \
      -projectPath $project_path \
      -importPackage "$package_path/Bugsnag.unitypackage"
    RESULT=$?
    if [ $RESULT -ne 0 ]; then exit $RESULT; fi
    exit 0

#    cp -R NativeCrashy.bundle maze_runner/Assets/Plugins/OSX/NativeCrashy.bundle

#    Unity -nographics -quit -batchmode -logFile $log_file \
#      -projectPath $project_path \
#      -executeMethod "Main.CreateScene"
#    RESULT=$?
#    if [ $RESULT -ne 0 ]; then exit $RESULT; fi

#    Unity -nographics -quit -batchmode -logFile - \
#      -projectPath $project_path \
#      -buildOSXUniversalPlayer "$(pwd)/Mazerunner.app"
#    RESULT=$?
#    if [ $RESULT -ne 0 ]; then exit $RESULT; fi
  popd
popd


    Unity -nographics -quit -batchmode -projectPath "fixtures/maze_runner" -importPackage "../../../Bugsnag.unitypackage"
