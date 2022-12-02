#!/bin/bash -e

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
elif [ "$1" == "wsl" ]; then
  PLATFORM="Win64"
  set -m
  UNITY_PATH="/mnt/c/Program Files/Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity.exe"
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
    root_path=`pwd`
  popd
  pushd ../fixtures

    import_log_file="$root_path/unity_import.log"
    log_file="$root_path/unity.log"
    package_path="$root_path/Bugsnag.unitypackage"
    project_path="$root_path/features/fixtures/maze_runner"

    if [ "$1" == "wsl" ]; then
      # Solves an issue on WSL were wslpath fails if the file does not exist.
      if [ ! -f "$import_log_file" ]; then
        touch $import_log_file
      fi

      if [ ! -f "$log_file" ]; then
        touch $log_file
      fi

      import_log_file=`wslpath -w "$import_log_file"`
      log_file=`wslpath -w "$log_file"`
      package_path=`wslpath -w "$package_path"`
      project_path=`wslpath -w "$project_path"`
    fi

    echo "Expecting to find Bugsnag package in: $package_path"

    # Run unity and immediately exit afterwards, log all output
    DEFAULT_CLI_ARGS="-nographics -quit -batchmode"

    echo "Importing $package_path into $project_path"
    echo "$UNITY_PATH" $DEFAULT_CLI_ARGS \
      -logFile $import_log_file \
      -projectPath "$project_path" \
      -ignoreCompilerErrors \
      -importPackage "$package_path"

    "$UNITY_PATH" $DEFAULT_CLI_ARGS \
      -logFile $import_log_file \
      -projectPath "$project_path" \
      -ignoreCompilerErrors \
      -importPackage "$package_path"
    RESULT=$?
    if [ $RESULT -ne 0 ]; then exit $RESULT; fi

    echo "Building the fixture for $PLATFORM"

    echo "$UNITY_PATH" $DEFAULT_CLI_ARGS \
      -logFile "$log_file" \
      -projectPath "$project_path" \
      -executeMethod "Builder.$PLATFORM"
    "$UNITY_PATH" $DEFAULT_CLI_ARGS \
      -logFile "$log_file" \
      -projectPath "$project_path" \
      -executeMethod "Builder.$PLATFORM"
    RESULT=$?
    if [ $RESULT -ne 0 ]; then exit $RESULT; fi
  popd
popd
