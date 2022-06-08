#!/usr/bin/env bash

if [ -z "$UNITY_PATH" ]
then
  echo "UNITY_PATH must be set, to e.g. /c/Program\ Files/Unity/Hub/Editor/2021.3.1f1/Editor/Unity.exe"
  exit 1
fi

echo "\`which Unity\`=`which Unity`"

pushd "${0%/*}"
  script_path=`pwd`
popd

pushd "$script_path/../../fixtures"

# Run unity and immediately exit afterwards, log all output
DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile unity.log"

project_path=`pwd`/maze_runner

# Build for Android

$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod Builder.SwitchBuild
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

#mv $project_path/mazerunner.apk $project_path/mazerunner_$UNITY_VERSION.apk
