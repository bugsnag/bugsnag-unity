#!/usr/bin/env bash

if [ -z "$UNITY_VERSION" ]; then
  echo "UNITY_VERSION must be set, to e.g. 2017.4.40f1"
  exit 1
fi


#!/usr/bin/env bash

if [ "$(uname)" == "Darwin" ]; then
   PLATFORM="MacOS"    
elif [ "$(expr substr $(uname -s) 1 10)" == "MINGW64_NT" ]; then
    PLATFORM="Win64"  
fi

if [ "$PLATFORM" == "MacOS" ]; then

  echo "MacOS Detected"
  UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity"

else

  set -m
  echo "Windows64 Detected"
  UNITY_PATH="/c/Program Files/Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity.exe" 

fi

# Set project_path to the repo root
pushd "${0%/*}"
  pushd ../../../..
    package_path=`pwd`
    echo "Expecting to find Bugsnag package in: $package_path"
  popd
  pushd ../fixtures
    log_file="$package_path/unity.log"
    project_path="$(pwd)/maze_runner"

    # Run unity and immediately exit afterwards, log all output
    DEFAULT_CLI_ARGS="-nographics -quit -batchmode -logFile $log_file"

    if [[ "$UNITY_PATH" == *"2020"* ]]; then
      # Copy the 2020 package manifest in for that version
      mkdir -p maze_runner/Packages
      cp manifests/manifest_2020.json maze_runner/Packages/manifest.json
    elif [[ "$UNITY_PATH" == *"2019"* ]]; then
      # Copy the 2019 package manifest in for that version
      mkdir -p maze_runner/Packages
      cp manifests/manifest_2019.json maze_runner/Packages/manifest.json
    else
      # On all other versions disable the package manager
      DEFAULT_CLI_ARGS="${DEFAULT_CLI_ARGS} -noUpm"
    fi

    echo "Importing $package_path/Bugsnag.unitypackage into $project_path"
    "$UNITY_PATH" $DEFAULT_CLI_ARGS \
      -projectPath $project_path \
      -ignoreCompilerErrors \
      -importPackage "$package_path/Bugsnag.unitypackage"
    RESULT=$?
    if [ $RESULT -ne 0 ]; then exit $RESULT; fi

    app_location="$(pwd)/Mazerunner.app"
    echo "Building $app_location"

    "$UNITY_PATH" $DEFAULT_CLI_ARGS \
      -projectPath $project_path \
      -executeMethod "Builder.$PLATFORM"
    RESULT=$?
    if [ $RESULT -ne 0 ]; then exit $RESULT; fi


    if [ "$PLATFORM" == "MacOS" ]; then

       tar -czf "Mazerunner-$UNITY_VERSION.app.zip" "maze_runner/Mazerunner.app"
        RESULT=$?
         if [ $RESULT -ne 0 ]; then exit $RESULT; fi

    else
      echo "SHOULD ZIP BUILD"
       tar -czf "Mazerunner-$UNITY_VERSION.zip" "maze_runner/WindowsBuild/"
        RESULT=$?
         if [ $RESULT -ne 0 ]; then exit $RESULT; fi

    fi



   
  popd
popd
