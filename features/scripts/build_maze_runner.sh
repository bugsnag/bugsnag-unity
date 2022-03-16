#!/usr/bin/env bash

if [ -z "$UNITY_VERSION" ]; then
  echo "UNITY_VERSION must be set, to e.g. 2018.4.36f1"
  exit 1
fi

if [[ $# != 1 ]]; then
  echo "Build platform (macos/webgl/windows) must be passed as a parameter"
  exit 2
fi

if [ "$1" == "macos" ]; then
  PLATFORM="MacOS"
  UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity"
elif [ "$1" == "windows" ]; then
  PLATFORM="Win64"
  set -m
  UNITY_PATH="/c/Program Files/Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity.exe"
elif [ "$1" == "webgl" ]; then
  PLATFORM="WebGL"
  if [ "$(uname)" == "Darwin" ]; then
    UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity"
  else
    UNITY_PATH="/c/Program Files/Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity.exe"
  fi
else
  echo "Unsupported platform: $1"
  exit 3
fi

# Set project_path to the repo root
SCRIPT_DIR=$(dirname "$(realpath $0)")
pushd $SCRIPT_DIR
  pushd ../..
    package_path=`pwd`
    echo "Expecting to find Bugsnag package in: $package_path"
  popd
  pushd ../fixtures
    import_log_file="$package_path/unity_import.log"
    log_file="$package_path/unity.log"
    project_path="$(pwd)/maze_runner"

    # Run unity and immediately exit afterwards, log all output
    DEFAULT_CLI_ARGS="-nographics -quit -batchmode"

    # if [[ "$UNITY_PATH" == *"2021"* ]]; then
    #   # Copy the 2021 package manifest in for that version
    #   mkdir -p maze_runner/Packages
    #   cp manifests/manifest_2021.json maze_runner/Packages/manifest.json
    # elif [[ "$UNITY_PATH" == *"2020"* ]]; then
    #   # Copy the 2020 package manifest in for that version
    #   mkdir -p maze_runner/Packages
    #   cp manifests/manifest_2020.json maze_runner/Packages/manifest.json
    # elif [[ "$UNITY_PATH" == *"2019"* ]]; then
    #   # Copy the 2019 package manifest in for that version
    #   mkdir -p maze_runner/Packages
    #   cp manifests/manifest_2019.json maze_runner/Packages/manifest.json
    # else
    #   # On all other versions disable the package manager
    #   DEFAULT_CLI_ARGS="${DEFAULT_CLI_ARGS} -noUpm"
    # fi

    echo "Importing $package_path/Bugsnag.unitypackage into $project_path"
    "$UNITY_PATH" $DEFAULT_CLI_ARGS \
      -logFile $import_log_file \
      -projectPath $project_path \
      -ignoreCompilerErrors \
      -importPackage "$package_path/Bugsnag.unitypackage"
    RESULT=$?
    if [ $RESULT -ne 0 ]; then exit $RESULT; fi

    echo "Building the fixture for $PLATFORM"

    "$UNITY_PATH" $DEFAULT_CLI_ARGS \
      -logFile $log_file \
      -projectPath $project_path \
      -executeMethod "Builder.$PLATFORM"
    RESULT=$?
    if [ $RESULT -ne 0 ]; then exit $RESULT; fi
  popd
popd
