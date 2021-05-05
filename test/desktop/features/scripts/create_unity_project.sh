#!/usr/bin/env bash

export PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS:$PATH"

# Set project_path to the repo root
pushd "${0%/*}"
  pushd ../../../..
    package_path=`pwd`
    echo "Expecting to find Busnag package in: $package_path"
  popd
  pushd ../fixtures
    git clean -xdf .
    log_file="$package_path/unity.log"
    project_path="$(pwd)/unity_project"

    echo "Creating Unity project in $project_path"
    Unity -nographics -quit -batchmode -logFile $log_file \
      -createProject $project_path

    echo "Importing Bugsnag package $package_path/Bugsnag.unitypackage"
    Unity -nographics -quit -batchmode -logFile $log_file \
      -projectPath $project_path \
      -importPackage "$package_path/Bugsnag.unitypackage"
    RESULT=$?
    if [ $RESULT -ne 0 ]; then exit $RESULT; fi

    cp Main.cs unity_project/Assets/Main.cs
    cp -R NativeCrashy.bundle unity_project/Assets/Plugins/OSX/NativeCrashy.bundle

    Unity -nographics -quit -batchmode -logFile $log_file \
      -projectPath $project_path \
      -executeMethod "Main.CreateScene"
    RESULT=$?
    if [ $RESULT -ne 0 ]; then exit $RESULT; fi

    Unity -nographics -quit -batchmode -logFile - \
      -projectPath $project_path \
      -buildOSXUniversalPlayer "$(pwd)/Mazerunner-${UNITY_VERSION}.app"
    RESULT=$?
    if [ $RESULT -ne 0 ]; then exit $RESULT; fi
  popd
popd
