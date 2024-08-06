#!/bin/bash -e

if [ -z "$UNITY_VERSION" ]; then
  echo "UNITY_VERSION must be set, to e.g. 2018.4.36f1"
  exit 1
fi

if [[ $# != 2 ]]; then
  echo "Build type (release/dev) and platform (macos/webgl/windows/wsl/linux) must be passed as parameters"
  exit 2
fi

BUILD_TYPE=$1
PLATFORM_TYPE=$2

if [ "$PLATFORM_TYPE" == "macos" ]; then
  if [ "$BUILD_TYPE" == "release" ]; then
    PLATFORM="MacOSRelease"
  else
    PLATFORM="MacOSDev"
  fi
  UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity"
elif [ "$PLATFORM_TYPE" == "windows" ]; then
  if [ "$BUILD_TYPE" == "release" ]; then
    PLATFORM="Win64Release"
  else
    PLATFORM="Win64Dev"
  fi
  set -m
  UNITY_PATH="/c/Program Files/Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity.exe"
elif [ "$PLATFORM_TYPE" == "wsl" ]; then
  if [ "$BUILD_TYPE" == "release" ]; then
    PLATFORM="Win64Release"
  else
    PLATFORM="Win64Dev"
  fi
  set -m
  UNITY_PATH="/mnt/c/Program Files/Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity.exe"
elif [ "$PLATFORM_TYPE" == "linux" ]; then
  if [ "$BUILD_TYPE" == "release" ]; then
    PLATFORM="Linux64Release"
  else
    PLATFORM="Linux64Dev"
  fi
  set -m
  UNITY_PATH="$HOME/Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity"
elif [ "$PLATFORM_TYPE" == "webgl" ]; then
  if [ "$BUILD_TYPE" == "release" ]; then
    PLATFORM="WebGLRelease"
  else
    PLATFORM="WebGLDev"
  fi
  if [ "$(uname)" == "Darwin" ]; then
    UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity"
  else
    UNITY_PATH="/c/Program Files/Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity.exe"
  fi
else
  echo "Unsupported platform: $PLATFORM_TYPE"
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

    if [ "$PLATFORM_TYPE" == "wsl" ]; then
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
