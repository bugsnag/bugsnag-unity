#!/usr/bin/env bash

export PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS:$PATH"

# Set project_path to the repo root
pushd "${0%/*}"
  pushd ../../../..
    package_path=`pwd`
    echo "Expecting to find Bugsnag package in: $package_path"
  popd
  pushd ../fixtures
    log_file="$package_path/unity.log"
    project_path="$(pwd)/maze_runner"

    echo "Importing $package_path/Bugsnag.unitypackage into $project_path"
    Unity -nographics -quit -batchmode \
      -logFile $log_file \
      -projectPath $project_path \
      -ignoreCompilerErrors \
      -importPackage "$package_path/Bugsnag.unitypackage"
    RESULT=$?
    if [ $RESULT -ne 0 ]; then exit $RESULT; fi

    app_location="$(pwd)/Mazerunner.app"
    echo "Building $app_location"

    Unity -nographics -quit -batchmode \
      -logFile $log_file \
      -projectPath $project_path \
      -executeMethod Builder.MacOSBuild
    RESULT=$?
    if [ $RESULT -ne 0 ]; then exit $RESULT; fi

    tar -czf "Mazerunner-$UNITY_VERSION.app.zip" "maze_runner/Mazerunner.app"
    RESULT=$?
     if [ $RESULT -ne 0 ]; then exit $RESULT; fi
  popd
popd
