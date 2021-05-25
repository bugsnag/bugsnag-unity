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

# Run unity and immediately exit afterwards, log all output
DEFAULT_CLI_ARGS="-quit -batchmode -logFile unity.log"
# Disable the package manager only on Unity 2018 due to dependency clashes
if [[ "$UNITY_PATH" == *"2018"* ]]; then
  DEFAULT_CLI_ARGS += " -noUpm"
fi

project_path=`pwd`/maze_runner

# Build for Android
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod Builder.AndroidBuild
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

mv $project_path/mazerunner.apk $project_path/mazerunner_$UNITY_VERSION.apk
